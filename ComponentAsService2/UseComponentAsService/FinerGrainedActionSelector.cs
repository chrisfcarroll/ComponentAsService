using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;

namespace ComponentAsService2.UseComponentAsService
{
    public class FinerGrainedActionSelector : ActionSelector
    {
        public delegate ActionDescriptor SelectBestOneActionDelegate(ILogger logger, IReadOnlyList<ActionDescriptor> actions);

        readonly ILogger<FinerGrainedActionSelector> logger;

        /// <summary>
        /// Creates a new <see cref="FinerGrainedActionSelector"/>, which
        /// overrides <see cref="Microsoft.AspNetCore.Mvc.Internal.ActionSelector.SelectBestActions" />.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">
        /// The <see cref="T:Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider" />.
        /// </param>
        /// <param name="actionConstraintCache">The <see cref="T:Microsoft.AspNetCore.Mvc.Internal.ActionConstraintCache" /> that
        /// providers a set of <see cref="T:Microsoft.AspNetCore.Mvc.ActionConstraints.IActionConstraint" /> instances.</param>
        /// <param name="loggerFactory">The <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" />.</param>
        /// <remarks>The constructor parameters are passed up to the base constructor</remarks>
        public FinerGrainedActionSelector(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            ILoggerFactory loggerFactory) : base(actionDescriptorCollectionProvider, actionConstraintCache, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<FinerGrainedActionSelector>();
        }

        /// <summary>This Strategy is called by <see cref="SelectBestActions"/> if and only if there is
        /// more than one action to choose from. 
        /// The default implementation simply picks the first one.
        /// </summary>
        public SelectBestOneActionDelegate SelectActionStrategy
        {
            get => _selectActionStrategy;
            set => _selectActionStrategy = value ?? SelectActionByParameterNameAndConvertibility.Apply;
        }
        SelectBestOneActionDelegate _selectActionStrategy = SelectActionByParameterNameAndConvertibility.Apply;


        /// <inheritdoc />
        /// <summary>Calls <see cref="SelectActionStrategy"/> to returns the single best matching action.</summary>
        /// <param name="actions">The set of actions that satisfy all constraints.</param>
        /// <returns>A list of the best matching actions.</returns>
        protected override IReadOnlyList<ActionDescriptor> SelectBestActions(IReadOnlyList<ActionDescriptor> actions)
            => actions?.Count>1 ? new []{ _selectActionStrategy(logger, actions)} : actions;
    }
}