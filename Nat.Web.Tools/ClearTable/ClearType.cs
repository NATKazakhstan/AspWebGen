namespace Nat.Web.Tools
{
    /// <summary>
    /// ��� ������ ������.
    /// </summary>
    public enum ClearType
    {
        /// <summary>
        /// ��� ������� �� ����������.
        /// </summary>
        NotSet,
        /// <summary>
        /// ��� �������.
        /// </summary>
        All,
        /// <summary>
        /// �� �������.
        /// </summary>
        Not,
        /// <summary>
        /// ������� ������ �� �������, ���� ��� �������� �����.
        /// </summary>
        ChildsNotExist,
        /// <summary>
        /// ������� ������ �� �������, ���� ��� ������������ �����.
        /// </summary>
        ParentNotExist
    }
}