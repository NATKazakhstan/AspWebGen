/*
 * Created by: Denis M. Silkov
 * Created: 1 ������� 2007 �.
 * Copyright � JSC New Age Technologies 2007
 */

using System;
using System.Collections.Generic;
using System.Web.UI;
using Nat.Tools.Classes;

namespace Nat.Web.Tools
{
    /// <summary>
    /// ��������� ��������, ���������� ����������(�) html-��������.
    /// </summary>
    public interface IClientElementProvider
    {
        /// <summary>
        /// ���������� �������������� html-���������, � ������� �������������� ���� ������.
        /// </summary>
        /// <returns></returns>
        ICollection<string> GetInputElements();

        /// <summary>
        /// ���������� ��������, � ������� �������������� ���� ������.
        /// </summary>
        /// <returns></returns>
        ICollection<Control> GetInputControls();

        /// <summary>
        /// ���������� ���������� �������� ��� html-���������, � ������� �������������� ���� ������.
        /// </summary>
        /// <returns>������ ��������: ������������� ��������, ��������� �������, ��������� ������ (�������������).</returns>
        ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders();
    }
}