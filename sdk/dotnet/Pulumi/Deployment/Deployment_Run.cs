﻿// Copyright 2016-2019, Pulumi Corporation

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Pulumi.Testing;

namespace Pulumi
{
    public partial class Deployment
    {
        /// <summary>
        /// <see cref="RunAsync(Func{Task{IDictionary{string,object}}}, StackOptions)"/> for more details.
        /// </summary>
        /// <param name="action">Callback that creates stack resources.</param>
        public static Task<int> RunAsync(Action action)
            => RunAsync(() =>
            {
                action();
                return ImmutableDictionary<string, object?>.Empty;
            });

        /// <summary>
        /// <see cref="RunAsync(Func{Task{IDictionary{string, object}}}, StackOptions)"/> for more details.
        /// </summary>
        /// <param name="func">Callback that creates stack resources.</param>
        /// <returns>A dictionary of stack outputs.</returns>
        public static Task<int> RunAsync(Func<IDictionary<string, object?>> func)
            => RunAsync(() => Task.FromResult(func()));

        /// <summary>
        /// <see cref="RunAsync(Func{Task{IDictionary{string, object}}}, StackOptions)"/> for more details.
        /// </summary>
        /// <param name="func">Callback that creates stack resources.</param>
        public static Task<int> RunAsync(Func<Task> func)
            => RunAsync(async () =>
            {
                await func().ConfigureAwait(false);
                return ImmutableDictionary<string, object?>.Empty;
            });

        /// <summary>
        /// <see cref="RunAsync(Func{Task{IDictionary{string, object}}}, StackOptions)"/> is an
        /// entry-point to a Pulumi application. .NET applications should perform all startup logic
        /// they need in their <c>Main</c> method and then end with:
        /// <para>
        /// <c>
        /// static Task&lt;int&gt; Main(string[] args)
        /// {
        ///     // program initialization code ...
        ///
        ///     return Deployment.Run(async () =>
        ///     {
        ///         // Code that creates resources.
        ///     });
        /// }
        /// </c>
        /// </para>
        /// Importantly: Cloud resources cannot be created outside of the lambda passed to any of the
        /// <see cref="Deployment.RunAsync(Action)"/> overloads.  Because cloud Resource construction is
        /// inherently asynchronous, the result of this function is a <see cref="Task{T}"/> which should
        /// then be returned or awaited.  This will ensure that any problems that are encountered during
        /// the running of the program are properly reported.  Failure to do this may lead to the
        /// program ending early before all resources are properly registered.
        /// <para/>
        /// The function passed to <see cref="RunAsync(Func{Task{IDictionary{string, object}}}, StackOptions)"/>
        /// can optionally return an <see cref="IDictionary{TKey, TValue}"/>.  The keys and values
        /// in this dictionary will become the outputs for the Pulumi Stack that is created.
        /// </summary>
        /// <param name="func">Callback that creates stack resources.</param>
        /// <param name="options">Stack options.</param>
        public static Task<int> RunAsync(Func<Task<IDictionary<string, object?>>> func, StackOptions? options = null)
            => CreateRunnerAndRunAsync(() => new Deployment(), runner => runner.RunAsync(func, options));

        /// <summary>
        /// <see cref="RunAsync{TStack}()"/> is an entry-point to a Pulumi
        /// application. .NET applications should perform all startup logic they
        /// need in their <c>Main</c> method and then end with:
        /// <para>
        /// <c>
        /// static Task&lt;int&gt; Main(string[] args) {// program
        /// initialization code ...
        ///
        ///     return Deployment.Run&lt;MyStack&gt;();}
        /// </c>
        /// </para>
        /// <para>
        /// Deployment will instantiate a new stack instance based on the type
        /// passed as TStack type parameter. Importantly, cloud resources cannot
        /// be created outside of the <see cref="Stack"/> component.
        /// </para>
        /// <para>
        /// Because cloud Resource construction is inherently asynchronous, the
        /// result of this function is a <see cref="Task{T}"/> which should then
        /// be returned or awaited.  This will ensure that any problems that are
        /// encountered during the running of the program are properly reported.
        /// Failure to do this may lead to the program ending early before all
        /// resources are properly registered.
        /// </para>
        /// </summary>
        public static Task<int> RunAsync<TStack>() where TStack : Stack, new()
            => CreateRunnerAndRunAsync(() => new Deployment(), runner => runner.RunAsync<TStack>());

        /// <summary>
        /// <see cref="RunAsync{TStack}()"/> is an entry-point to a Pulumi
        /// application. .NET applications should perform all startup logic they
        /// need in their <c>Main</c> method and then end with:
        /// <para>
        /// <c>
        /// static Task&lt;int&gt; Main(string[] args) {// program
        /// initialization code ...
        ///
        ///     return Deployment.Run&lt;MyStack&gt;(serviceProvider);}
        /// </c>
        /// </para>
        /// <para>
        /// Deployment will instantiate a new stack instance based on the type
        /// passed as TStack type parameter using the serviceProvider.
        /// Importantly, cloud resources cannot be created outside of the
        /// <see cref="Stack"/> component.
        /// </para>
        /// <para>
        /// Because cloud Resource construction is inherently asynchronous, the
        /// result of this function is a <see cref="Task{T}"/> which should then
        /// be returned or awaited.  This will ensure that any problems that are
        /// encountered during the running of the program are properly reported.
        /// Failure to do this may lead to the program ending early before all
        /// resources are properly registered.
        /// </para>
        /// </summary>
        public static Task<int> RunAsync<TStack>(IServiceProvider serviceProvider) where TStack : Stack
            => CreateRunnerAndRunAsync(() => new Deployment(), runner => runner.RunAsync<TStack>(serviceProvider));

        /// <summary>
        /// Entry point to test a Pulumi application. Deployment will
        /// instantiate a new stack instance based on the type passed as TStack
        /// type parameter using the given service provider. This method creates
        /// no real resources.
        /// Note: Currently, unit tests that call
        /// <see cref="TestWithServiceProviderAsync{TStack}(IMocks, IServiceProvider, TestOptions)"/>
        /// must run serially; parallel execution is not supported.
        /// </summary>
        /// <param name="mocks">Hooks to mock the engine calls.</param>
        /// <param name="serviceProvider"></param>
        /// <param name="options">Optional settings for the test run.</param>
        /// <typeparam name="TStack">The type of the stack to test.</typeparam>
        /// <returns>Test result containing created resources and errors, if any.</returns>
        public static Task<ImmutableArray<Resource>> TestWithServiceProviderAsync<TStack>(IMocks mocks, IServiceProvider serviceProvider, TestOptions? options = null)
            where TStack : Stack
            => TestAsync(mocks, runner => runner.RunAsync<TStack>(serviceProvider), options);

        /// <summary>
        /// Entry point to test a Pulumi application. Deployment will
        /// instantiate a new stack instance based on the type passed as TStack
        /// type parameter. This method creates no real resources.
        /// Note: Currently, unit tests that call <see cref="TestAsync{TStack}(IMocks, TestOptions)"/>
        /// must run serially; parallel execution is not supported.
        /// </summary>
        /// <param name="mocks">Hooks to mock the engine calls.</param>
        /// <param name="options">Optional settings for the test run.</param>
        /// <typeparam name="TStack">The type of the stack to test.</typeparam>
        /// <returns>Test result containing created resources and errors, if any.</returns>
        public static Task<ImmutableArray<Resource>> TestAsync<TStack>(IMocks mocks, TestOptions? options = null)
            where TStack : Stack, new()
            => TestAsync(mocks, runner => runner.RunAsync<TStack>(), options);

        /// <summary>
        /// Used inside TestAsync overloads where users have a function that creates resources
        /// in which case an internal TestStack is used to create the resources.
        ///
        /// This function takes the created resources from the TestStack and filters it out of the created resources
        /// (since it is internal) and obtains the outputs returned, if any from that TestStack. 
        /// </summary>
        /// <param name="resources">The created resources from TestAsync</param>
        /// <returns>Resources and outputs</returns>
        private static (ImmutableArray<Resource> Resources, IDictionary<string, object?> Outputs) TestResults(
            ImmutableArray<Resource> resources)
        {
            var result = new List<Resource>();
            IDictionary<string, object?> outputs = new Dictionary<string, object?>(); 
            foreach (var resource in resources)
            {
                if (resource is TestStack testStack)
                {
                    // Obtain the outputs from the test stack
                    outputs = testStack.Outputs;
                    // Since TestStack is internal, Skip adding it as part of the resources created by the callback
                    continue;
                }
                
                result.Add(resource);
            }

            return (result.ToImmutableArray(), outputs);
        }
        
        /// <summary>
        /// Entry point to test a Pulumi application. Deployment will
        /// run the provided function that creates resources but doesn't actually deploy them
        /// Note: Currently, unit tests that call this function 
        /// must run serially; parallel execution is not supported.
        /// </summary>
        /// <param name="testMocks">Hooks to mock the engine calls.</param>
        /// <param name="testOptions">Optional settings for the test run.</param>
        /// <param name="createResources">The function which creates resources and returns outputs.</param>
        /// <returns>Test result containing created resources and outputs, if any.</returns>
        public static async Task<(ImmutableArray<Resource> Resources, IDictionary<string, object?> Outputs)> TestAsync(
            IMocks testMocks,
            TestOptions testOptions,
            Func<Task<IDictionary<string, object?>>> createResources)
        {
            var createdResources = await TestAsync(
                mocks: testMocks,
                runAsync: runner => runner.RunAsync(() => new TestStack(createResources)), 
                testOptions);

            return TestResults(createdResources);
        }
        
        /// <summary>
        /// Entry point to test a Pulumi application. Deployment will
        /// run the provided function that creates resources but doesn't actually deploy them
        /// Note: Currently, unit tests that call this function 
        /// must run serially; parallel execution is not supported.
        /// </summary>
        /// <param name="testMocks">Hooks to mock the engine calls.</param>
        /// <param name="testOptions">Optional settings for the test run.</param>
        /// <param name="createResources">The function which creates resources and returns outputs.</param>
        /// <returns>Test result containing created resources and outputs, if any.</returns>
        public static async Task<(ImmutableArray<Resource> Resources, IDictionary<string, object?> Outputs)> TestAsync(
            IMocks testMocks,
            TestOptions testOptions,
            Func<IDictionary<string, object?>> createResources)
        {
            var createdResources = await TestAsync(
                mocks: testMocks,
                runAsync: runner => runner.RunAsync(() => new TestStack(createResources)), 
                testOptions);

            return TestResults(createdResources);
        }
        
        /// <summary>
        /// Entry point to test a Pulumi application. Deployment will
        /// run the provided function that creates resources but doesn't actually deploy them
        /// Note: Currently, unit tests that call this function 
        /// must run serially; parallel execution is not supported.
        /// </summary>
        /// <param name="testMocks">Hooks to mock the engine calls.</param>
        /// <param name="testOptions">Optional settings for the test run.</param>
        /// <param name="createResources">The function which creates resources and returns outputs.</param>
        /// <returns>Test result containing created resources and outputs, if any.</returns>
        public static async Task<ImmutableArray<Resource>> TestAsync(
            IMocks testMocks,
            TestOptions testOptions,
            Func<Task> createResources)
        {
            var createdResources = await TestAsync(
                mocks: testMocks,
                runAsync: runner => runner.RunAsync(() => new TestStack(createResources)), 
                testOptions);

            return TestResults(createdResources).Resources;
        }
        
        /// <summary>
        /// Entry point to test a Pulumi application. Deployment will
        /// run the provided function that creates resources but doesn't actually deploy them
        /// Note: Currently, unit tests that call this function 
        /// must run serially; parallel execution is not supported.
        /// </summary>
        /// <param name="testMocks">Hooks to mock the engine calls.</param>
        /// <param name="testOptions">Optional settings for the test run.</param>
        /// <param name="createResources">The function which creates resources and returns outputs.</param>
        /// <returns>Test result containing created resources and outputs, if any.</returns>
        public static async Task<ImmutableArray<Resource>> TestAsync(
            IMocks testMocks,
            TestOptions testOptions,
            Action createResources)
        {
            var createdResources = await TestAsync(
                mocks: testMocks,
                runAsync: runner => runner.RunAsync(() => new TestStack(createResources)), 
                testOptions);

            return TestResults(createdResources).Resources;
        }

        private static async Task<ImmutableArray<Resource>> TestAsync(IMocks mocks, Func<IRunner, Task<int>> runAsync, TestOptions? options = null)
        {
            var result = await TryTestAsync(mocks, runAsync, options);
            if (result.Exception != null)
            {
                throw result.Exception;
            }
            return result.Resources;
        }

        /// <summary>
        /// Like `TestAsync`, but instead of throwing the errors
        /// detected in the engine, returns them in the result tuple.
        /// This enables tests to observe partially constructed
        /// `Resources` vector in presence of deliberate errors.
        /// </summary>
        internal static async Task<(ImmutableArray<Resource> Resources, Exception? Exception)> TryTestAsync(
            IMocks mocks, Func<IRunner, Task<int>> runAsync, TestOptions? options = null)
        {
            var engine = new MockEngine();
            var monitor = new MockMonitor(mocks);
            await CreateRunnerAndRunAsync(() => new Deployment(engine, monitor, options), runAsync).ConfigureAwait(false);
            Exception? err = engine.Errors.Count switch
            {
                1 => new RunException(engine.Errors.Single()),
                var v when v > 1 => new AggregateException(engine.Errors.Select(e => new RunException(e))),
                _ => null
            };
            return (Resources: monitor.Resources.ToImmutableArray(), Exception: err);
        }

        internal static Task<(ImmutableArray<Resource> Resources, Exception? Exception)> TryTestAsync<TStack>(
            IMocks mocks, TestOptions? options = null)
            where TStack : Stack, new()
            => TryTestAsync(mocks, runner => runner.RunAsync<TStack>(), options);

        // this method *must* remain marked async
        // in order to protect the scope of the AsyncLocal Deployment.Instance we cannot elide the task (return it early)
        // if the task is returned early and not awaited, than it is possible for any code that runs before the eventual await
        // to be executed synchronously and thus have multiple calls to one of the Run methods affecting each others Deployment.Instance
        internal static async Task<int> CreateRunnerAndRunAsync(
            Func<Deployment> deploymentFactory,
            Func<IRunner, Task<int>> runAsync)
        {
            var deployment = deploymentFactory();
            Instance = new DeploymentInstance(deployment);
            return await runAsync(deployment._runner).ConfigureAwait(false);
        }
    }
}
