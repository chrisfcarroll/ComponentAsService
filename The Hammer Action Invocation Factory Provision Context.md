I find that AspNetCore.Mvc has carefully avoided the swirling mad vortex of the
HammerFactoryFactory pattern by using HammerFactoryProviders instead.

And, for good measure, carefully placed the HammerFactoryProvider in a
HammerActionInvokerProvider.

All good hammer action invocation factory provision requires a context, so it
goes like this:

 

If

-   the HammerActionInvokerProviderContext.ActionContext.ActionDescriptor is a
    HammerActionDescriptor

then

-   ask the HammerFactoryProvider

    -   to create a HammerFactory

        -   using the
            HammerActionInvokerProviderContext.ActionContext.ActionDescriptor

    -   *but first*

        -   *wrap* the
            HammerActionInvokerProviderContext.ActionContext.ActionDescriptor

            -   in a HammerContext

        -   which the HammerActionInvokerCache will unwrap

            -   for the HammerFactoryProvider's HammerFactory to use.

 

I think I got that right.

However, I'm a bit worried that this a red herring, and the piece of magic I'm
looking for is inside the HammerBinderDelegateProvider, where the
CreateHammerBinderDelegate method creates a HammerPropertyBinderFactory, and
gives it a propertyBindingInfo and a parameterBindingInfo which use the
ModelBindingFactory, the ModelMetaDataProvider, the HammerActionDescriptor, and
some options, to ... uh .... bind. It.
