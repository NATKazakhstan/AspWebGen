namespace Nat.Web.Controls
{
    public enum LogMessageType : long
    {
        None = 0,

        /// <summary>
        /// ��������� ����, ������ �����������
        /// ������ ��������
        /// </summary>
        SystemSerializationPageRequest = 149,

        /// <summary>
        /// �������
        /// ������ � ��
        /// </summary>
        SystemErrorInApp = 150,

        /// <summary>
        /// ���������, ������
        /// ��������
        /// </summary>
        SystemJobsActions = 700,

        /// <summary>
        /// ���������, ��������� �������������
        /// ���������� ��������
        /// </summary>
        SystemRVSSettingsSaveSettings = 701,

        /// <summary>
        /// ���������, ��������� �������������
        /// �������� ����������� ��������
        /// </summary>
        SystemRVSSettingsLoadSavedSettings = 702,

        /// <summary>
        /// ���������, ��������� �������������
        /// �������
        /// </summary>
        SystemRVSSettingsExport = 703,

        /// <summary>
        /// ���������, ��������� �������������
        /// ����������� �����
        /// </summary>
        SystemRVSSettingsDeniedAccess = 704,

        /// <summary>
        /// ���������, ��������� �������������
        /// �������� �������
        /// </summary>
        SystemRVSSettingsView = 705,
        
        /// <summary>
        /// ���������, ��������� �� �������� �����
        /// �������� �����
        /// SendMail (720)
        /// </summary>
        SystemMailSendMailInformation = 720,

        /// <summary>
        /// ���������, ��������� �� �������� �����
        /// �������� �����
        /// ErrorOnSend (721)
        /// </summary>
        SystemMailSendMailError = 721,
    }
}