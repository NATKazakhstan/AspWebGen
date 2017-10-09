namespace Nat.Web.Controls.GenerationClasses.BaseJournal.ColumnInitializers
{
    public class CloneDataColumnInitializer
    {
        private readonly BaseColumn baseColumn;

        private readonly BaseColumn cloneColumn;

        public CloneDataColumnInitializer(BaseColumn cloneColumn, BaseColumn baseColumn)
        {
            this.baseColumn = baseColumn;
            this.cloneColumn = cloneColumn;
        }

        public void Initizlise()
        {
            cloneColumn.Format = baseColumn.Format;
            cloneColumn.ColumnType = baseColumn.ColumnType;
            cloneColumn.GetRowsCountHandler = this.GetRowsCount;
            cloneColumn.GetValueHandler = this.GetValue;
            cloneColumn.GetNameHandler = this.GetName;
        }

        private int GetRowsCount(RenderContext context)
        {
            cloneColumn.DependenceColumns(context, baseColumn);
            return baseColumn.GetRowsCount(context.OtherColumns[baseColumn.ColumnName]);
        }

        private string GetName(RenderContext context)
        {
            return baseColumn.GetNameByContext(context);
        }

        private object GetValue(RenderContext context)
        {
            cloneColumn.DependenceColumns(context, baseColumn);
            return baseColumn.GetValueByContext(context);
        }
    }
}