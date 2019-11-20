using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Ext.Net;

namespace Nat.Web.Tools.ExtNet
{
    /// <summary>
    /// Класс, для конфигурации колонки грида, содержащей контролы (команды) с выпадающим меню
    /// </summary>
    public class GridCommandColumn : GridColumn
    {
        private List<GridCommand> gridCommands;
        /// <summary>
        /// Список команд, включаемых в колонку CommandColumn.
        /// У каждой команды по необходимости инициализировать: Icon, CommandName, Text, Menu, ToolTip.
        /// </summary>
        public List<GridCommand> GridCommands
        {
            get
            {
                return gridCommands ?? (gridCommands = new List<GridCommand>());
            }

            set
            {
                gridCommands = value;
            }
        }

        /// <summary>
        /// JS функция для рендеринга состава GridCommand на клиентской стороне.
        /// Сигнатура функции: function (grid, toolbar, rowIndex, record).
        /// </summary>
        public string PrepareToolbarFunction { get; set; }

        /// <summary>
        /// Клиентский JS код, выполняемый при событии "Command".
        /// Сигнатура функции события: function (item, command, record, recordIndex, cellIndex).
        /// </summary>
        public string ListenerCommandHandler { get; set; }

        public override ColumnBase CreateColumn()
        {
            return CreateCommandColumn();
        }

        public CommandColumn CreateCommandColumn()
        {
            var column = new CommandColumn()
            {
                ID = "CustomCommands",
                Width = new Unit(Width),
                MenuDisabled = true,
                Align = Alignment.Center,
                Locked = Locked
            };

            if (!string.IsNullOrEmpty(PrepareToolbarFunction))
                column.PrepareToolbar.Fn = PrepareToolbarFunction;

            if (!string.IsNullOrEmpty(ListenerCommandHandler))
                column.Listeners.Command.Handler = ListenerCommandHandler;

            column.PreRender += AddCommandItems;
            ConfigureColumnHandler?.Invoke(column);

            return column;

            void AddCommandItems(object sender, EventArgs e)
            {
                foreach (var command in GridCommands)
                {
                    column.Commands.Add(command);
                }
            }
        }
    }
}
