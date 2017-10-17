/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 27 мая 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SyncDbmlByScript
{
    [Serializable]
    public class SyncAssociation : BaseSync
    {
        XElement parentTable;
        XElement childTable;

        protected override bool Execute()
        {
            #region Change Parent Association

            if (!NotCreateRefToChilds)
            {
                if (!string.IsNullOrEmpty(RefToChilds))
                {
                    ActiveObject = ParentTable + "_ByMember";
                    SetAssociationValues(parentTable, parentTable == childTable, ChildTableType, ChildTableCollection, ParentTableColumn, ChildTableColumn, RefToChilds, AssociationName + "_ByMember", IsOneToOne, false, false);
                }
                ActiveObject = ParentTable;
                SetAssociationValues(parentTable, parentTable == childTable, ChildTableType, ChildTableCollection, ParentTableColumn, ChildTableColumn, "", AssociationName, IsOneToOne, false, true);
            }

            #endregion

            #region Change Child Association

            if (!string.IsNullOrEmpty(RefToParent))
            {
                ActiveObject = ParentTable + "_ByMember";
                SetAssociationValues(childTable, parentTable == childTable, ParentTableType, ParentTableType, ChildTableColumn, ParentTableColumn, RefToParent, AssociationName + "_ByMember", false, true, false);
            }
            ActiveObject = ParentTable;
            SetAssociationValues(childTable, parentTable == childTable, ParentTableType, ParentTableType, ChildTableColumn, ParentTableColumn, "", AssociationName, false, true, true);

            #endregion

            return true;
        }

        protected void SetAssociationValues(XElement table, bool isSameTable, string otherTableType, string otherTableCollection, string thisKey, string otherKey, string member, string associationName, bool isOneToOne, bool isForeignKey, bool isMustExists)
        {
            if (string.IsNullOrEmpty(member))
            {
                if (isSameTable && otherTableType.Equals(otherTableCollection, StringComparison.OrdinalIgnoreCase) && !isForeignKey)
                    otherTableCollection += "es";
                var keyColumn = isForeignKey ? thisKey : otherKey;
                if (!string.IsNullOrEmpty(keyColumn))
                    keyColumn = keyColumn.Split(',')[0];
                member = isOneToOne ? otherTableType + "_" + keyColumn : otherTableCollection + "_" + keyColumn;
            }
            var association = isSameTable
                ? GetAssociation(otherTableType, thisKey, otherKey, table, member)
                : GetAssociation(associationName, table, member) ?? GetAssociation(otherTableType, thisKey, otherKey, table, member);
            if (association == null && isMustExists) association = AddAssociation(table);
            else if (association == null) return;
            SetAttributeValue(association, "Name", associationName, false);
            SetAttributeValue(association, "ThisKey", thisKey);
            SetAttributeValue(association, "OtherKey", otherKey);
            SetAttributeValue(association, "Type", otherTableType);
            if(!isForeignKey)
                SetAttributeValue(association, "IsForeignKey", null);
            else
                SetAttributeValue(association, "IsForeignKey", "true");
            if (isOneToOne)
            {
                SetAttributeValue(association, "Member", member);
                association.SetAttributeValue("Cardinality", "One");
            }
            else
            {
                SetAttributeValue(association, "Member", member);
                association.SetAttributeValue("Cardinality", null);
            }
        }

        protected override bool Validate()
        {

            if (string.IsNullOrEmpty(ParentTableColumn))
                Error("Property 'ParentTableColumn' is not set");
            if (string.IsNullOrEmpty(ChildTableColumn))
                Error("Property 'ChildTableColumn' is not set");
            if (string.IsNullOrEmpty(ParentTable))
                Error("Property 'ParentTable' is not set");
            if (string.IsNullOrEmpty(ChildTable))
                Error("Property 'ChildTable' is not set");
            if (string.IsNullOrEmpty(ParentTableType))
                Error("Property 'ParentTableType' is not set");
            if (string.IsNullOrEmpty(ChildTableType))
                Error("Property 'ChildTableColumn' is not set");
            if (string.IsNullOrEmpty(ParentTableCollection))
                Error("Property 'ParentTableCollection' is not set");
            if (string.IsNullOrEmpty(ChildTableCollection))
                Error("Property 'ChildTableCollection' is not set");
            if (string.IsNullOrEmpty(AssociationName))
                Error("Property 'AssociationName' is not set");

            parentTable = EnsureExistsTable(ParentTable);
            childTable = EnsureExistsTable(ChildTable);
            if (parentTable == null)
                Error("Not found Parent Table '{0}'", ParentTable);
            if (childTable == null)
                Error("Not found Child Table '{0}'", ChildTable);

            return true;
        }

        public override string GetName()
        {
            if (string.IsNullOrEmpty(AssociationName))
                return base.GetName();
            return AssociationName;
        }

        protected override XElement GetObject()
        {
            throw new NotImplementedException();
        }

        public override bool IsChangeTables(IDictionary<string, string> tables)
        {
            return tables.ContainsKey(ParentTable) || tables.ContainsKey(ChildTable);
        }

        public string ParentTableColumn { get; set; }
        public string ChildTableColumn { get; set; }
        public string ParentTable { get; set; }
        public string ChildTable { get; set; }
        public string ParentTableType { get; set; }
        public string ChildTableType { get; set; }
        public string ParentTableCollection { get; set; }
        public string ChildTableCollection { get; set; }
        public string ParentMember { get; set; }
        public string ChildsMember { get; set; }
        public bool IsOneToOne { get; set; }
        public string AssociationName { get; set; }
        public string RefToChilds { get; set; }
        public string RefToParent { get; set; }
        public bool NotCreateRefToChilds { get; set; }
    }
}