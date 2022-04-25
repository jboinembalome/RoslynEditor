﻿using System.Windows.Input;

namespace RoslynCodeEditLib.Helpers
{
    /// <summary>
    /// Static class that contains the commands that can be executed by the <seealso cref="RoslynCodeEdit"/> control.
    /// </summary>
    public static class RoslynCodeEditCommands
    {
        /// <summary>
        /// The Collapse all folds commmand folds all text folds (if any) such that users
        /// can get an overview on the presented text (using a top to bottom approach).
        /// </summary>
        public static readonly RoutedCommand FoldsCollapseAll = new("CollapseAllFolds", typeof(RoslynCodeEdit)
          ////, new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) }
          );

        /// <summary>
        /// The Expand all folds commmand unfolds all text folds (if any) such that users
        /// can read all text items in a given text without having to worry about foldings.
        /// </summary>
        public static readonly RoutedCommand FoldsExpandAll = new("CollapseAllFolds", typeof(RoslynCodeEdit)
          ////, new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) }
          );
    }
}
