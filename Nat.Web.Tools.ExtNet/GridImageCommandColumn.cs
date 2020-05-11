using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Ext.Net;

namespace Nat.Web.Tools.ExtNet
{
    /// <summary>
    /// Класс, для конфигурации колонки грида, содержащей команды
    /// </summary>
    public class GridImageCommandColumn : GridColumn
    {
        private List<ImageCommand> imageCommands;
        /// <summary>
        /// Список команд, включаемых в колонку CommandColumn.
        /// У каждой команды по необходимости инициализировать: Icon, CommandName, Text, ToolTip.
        /// </summary>
        public List<ImageCommand> ImageCommands
        {
            get
            {
                return imageCommands ?? (imageCommands = new List<ImageCommand>());
            }

            set
            {
                imageCommands = value;
            }
        }

        /// <summary>
        /// JS функция для рендеринга состава ImageCommand на клиентской стороне.
        /// Сигнатура функции: function (grid, command, record, row).
        /// </summary>
        public string PrepareCommandFunction { get; set; }

        /// <summary>
        /// Клиентский JS код, выполняемый при событии "Command".
        /// Сигнатура функции события: function (item, command, record, recordIndex, cellIndex).
        /// </summary>
        public string ListenerCommandHandler { get; set; }

        public override ColumnBase CreateColumn()
        {
            return CreateCommandColumn();
        }

        public ImageCommandColumn CreateCommandColumn()
        {
            var column = new ImageCommandColumn()
            {
                ID = "CustomImageCommands",
                Width = new Unit(Width),
                MenuDisabled = true,
                Align = Alignment.Center,
                Locked = Locked
            };

            if (!string.IsNullOrEmpty(PrepareCommandFunction))
                column.PrepareCommand.Fn = PrepareCommandFunction;

            if (!string.IsNullOrEmpty(ListenerCommandHandler))
                column.Listeners.Command.Handler = ListenerCommandHandler;

            column.PreRender += AddCommandItems;
            ConfigureColumnHandler?.Invoke(column);

            return column;

            void AddCommandItems(object sender, EventArgs e)
            {
                foreach (var command in ImageCommands)
                {
                    column.Commands.Add(command);
                }
            }
        }
    }
}
