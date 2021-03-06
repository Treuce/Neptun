﻿using Neptun.Core;
using static Neptun.DI;

namespace Neptun
{
    /// <summary>
    /// Locates view models from the IoC for use in binding in Xaml files
    /// </summary>
    public class ViewModelLocator
    {
        #region Public Properties

        /// <summary>
        /// Singleton instance of the locator
        /// </summary>
        public static ViewModelLocator Instance { get; private set; } = new ViewModelLocator();

        /// <summary>
        /// The application view model
        /// </summary>
        public ApplicationViewModel ApplicationViewModel => ViewModelApplication;
        /// <summary>
        /// The view model for the schedule planner
        /// </summary>
        public ScheduleViewModel ScheduleViewModel => ScheduleVM;
        #endregion
    }
}
