global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Composer.Models;
global using Composer.Services;
global using Composer.Services.Endpoints;
global using Uno.Extensions.Reactive;
global using ApplicationExecutionState = Windows.ApplicationModel.Activation.ApplicationExecutionState;
[assembly: Uno.Extensions.Reactive.Config.BindableGenerationTool(3)]
