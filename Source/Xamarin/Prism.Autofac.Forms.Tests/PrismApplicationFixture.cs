﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Prism.Common;
using Prism.Autofac.Forms.Tests.Mocks;
using Prism.Autofac.Forms.Tests.Mocks.Modules;
using Prism.Autofac.Forms.Tests.Mocks.Services;
using Prism.Autofac.Forms.Tests.Mocks.ViewModels;
using Prism.Autofac.Forms.Tests.Mocks.Views;
using Prism.Autofac.Navigation;
using Prism.Navigation;
using Xamarin.Forms;
using Xunit;
using Autofac;
using Autofac.Core.Registration;
#if TEST
using Application = Prism.FormsApplication;
#endif

namespace Prism.Autofac.Forms.Tests
{
    public class PrismApplicationFixture
    {
        [Fact]
        public void OnInitialized()
        {
            var app = new PrismApplicationMock();
            Assert.True(app.Initialized);
        }

        [Fact]
        public void OnInitialized_SetPage()
        {
            var view = new ViewMock();
            var app = new PrismApplicationMock(view);
            Assert.True(app.Initialized);
            Assert.NotNull(Application.Current.MainPage);
            Assert.Same(view, Application.Current.MainPage);
        }

        [Fact]
        public void ResolveTypeRegisteredWithContainer()
        {
            var app = new PrismApplicationMock();
            var service = app.Container.Resolve<IAutofacServiceMock>();
            Assert.NotNull(service);
            Assert.IsType<AutofacServiceMock>(service);
        }

        [Fact]
        public void ResolveTypeRegisteredWithDependencyService()
        {
            var app = new PrismApplicationMock();
            // TODO
            // Since we must call Xamarin.Forms.Init() (and cannot do so from PCL)
            // to call Xamarin.Forms.DependencyService
            // we check that this throws an InvalidOperationException (for reason stated above).
            // This shows that a call to Xamarin.Forms.DependencyService was made and thus should return
            // service instance (if registered)
            Assert.Throws<ComponentNotRegisteredException>(
                () => app.Container.Resolve<IDependencyServiceMock>());
        }

        [Fact]
        public void Container_ResolveNavigationService()
        {
            var app = new PrismApplicationMock();
            var navigationService = app.NavigationService;
            Assert.NotNull(navigationService);
            Assert.IsType<AutofacPageNavigationService>(navigationService);
        }

        [Fact]
        public void Module_Initialize()
        {
            var app = new PrismApplicationMock();
            var module = app.Container.Resolve<ModuleMock>();
            Assert.NotNull(module);
            Assert.True(module.Initialized);
        }

        [Fact]
        public async Task Navigate_UnregisteredView_ThrowInvalidOperationException()
        {
            var app = new PrismApplicationMock();
            var navigationService = ResolveAndSetRootPage(app);
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await navigationService.NavigateAsync("missing"));
            Assert.Contains("missing", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Navigate_Key()
        {
            var app = new PrismApplicationMock();
            var navigationService = ResolveAndSetRootPage(app);
            await navigationService.NavigateAsync("view");
            var rootPage = ((IPageAware)navigationService).Page;
            Assert.True(rootPage.Navigation.ModalStack.Count == 1);
            Assert.IsType(typeof(ViewMock), rootPage.Navigation.ModalStack[0]);
        }

        [Fact]
        public void Navigate_ViewModelFactory_PageAware()
        {
            var app = new PrismApplicationMock();
            var view = new AutowireView();
            var viewModel = (AutowireViewModel)view.BindingContext;
            var pageAware = (IPageAware)viewModel.NavigationService;
            var navigationServicePage = app.CreateNavigationServiceForPage();
            Assert.IsType<AutowireView>(pageAware.Page);
            var navigatedPage = ((IPageAware)navigationServicePage).Page;
            Assert.IsType<AutowireView>(navigatedPage);
            Assert.Same(view, pageAware.Page);
            Assert.Same(pageAware.Page, navigatedPage);
        }

        [Fact]
        public void Container_ResolveByType()
        {
            var app = new PrismApplicationMock();
            var viewModel = app.Container.Resolve<ViewModelAMock>();
            Assert.NotNull(viewModel);
            Assert.IsType<ViewModelAMock>(viewModel);
        }

        [Fact]
        public void Container_ResolveByKey()
        {
            var app = new PrismApplicationMock();
            var viewModel = app.Container.ResolveNamed<ViewModelBMock>(ViewModelBMock.Key);
            Assert.NotNull(viewModel);
            Assert.IsType<ViewModelBMock>(viewModel);
        }

        private static INavigationService ResolveAndSetRootPage(PrismApplicationMock app)
        {
            var navigationService = app.NavigationService;
            ((IPageAware)navigationService).Page = new ContentPage();
            return navigationService;
        }
    }
}