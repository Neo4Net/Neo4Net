using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.extension
{

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using DependenciesProxy = Neo4Net.Kernel.impl.util.DependenciesProxy;
	using UnsatisfiedDependencyException = Neo4Net.Kernel.impl.util.UnsatisfiedDependencyException;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.stream;

	public abstract class AbstractKernelExtensions : Neo4Net.Graphdb.DependencyResolver_Adapter, Lifecycle
	{
		 private readonly KernelContext _kernelContext;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<KernelExtensionFactory<?>> kernelExtensionFactories;
		 private readonly IList<KernelExtensionFactory<object>> _kernelExtensionFactories;
		 private readonly Dependencies _dependencies;
		 private readonly LifeSupport _life = new LifeSupport();
		 private readonly KernelExtensionFailureStrategy _kernelExtensionFailureStrategy;

		 internal AbstractKernelExtensions<T1>( KernelContext kernelContext, IEnumerable<T1> kernelExtensionFactories, Dependencies dependencies, KernelExtensionFailureStrategy kernelExtensionFailureStrategy, ExtensionType extensionType )
		 {
			  this._kernelContext = kernelContext;
			  this._kernelExtensionFailureStrategy = kernelExtensionFailureStrategy;
			  this._kernelExtensionFactories = stream( kernelExtensionFactories ).filter( e => e.ExtensionType == extensionType ).collect( toList() );
			  this._dependencies = dependencies;
		 }

		 public override void Init()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (KernelExtensionFactory<?> kernelExtensionFactory : kernelExtensionFactories)
			  foreach ( KernelExtensionFactory<object> kernelExtensionFactory in _kernelExtensionFactories )
			  {
					try
					{
						 object kernelExtensionDependencies = GetKernelExtensionDependencies( kernelExtensionFactory );
						 Lifecycle dependency = NewInstance( _kernelContext, kernelExtensionFactory, kernelExtensionDependencies );
						 Objects.requireNonNull( dependency, kernelExtensionFactory.ToString() + " returned a null KernelExtension" );
						 _life.add( _dependencies.satisfyDependency( dependency ) );
					}
					catch ( UnsatisfiedDependencyException exception )
					{
						 _kernelExtensionFailureStrategy.handle( kernelExtensionFactory, exception );
					}
					catch ( Exception throwable )
					{
						 _kernelExtensionFailureStrategy.handle( kernelExtensionFactory, throwable );
					}
			  }

			  _life.init();
		 }

		 public override void Start()
		 {
			  _life.start();
		 }

		 public override void Stop()
		 {
			  _life.stop();
		 }

		 public override void Shutdown()
		 {
			  _life.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> T resolveDependency(Class<T> type, SelectionStrategy selector) throws IllegalArgumentException
		 public override T ResolveDependency<T>( Type type, SelectionStrategy selector )
		 {
				 type = typeof( T );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<? extends T> typeDependencies = resolveTypeDependencies(type);
			  IEnumerable<T> typeDependencies = ResolveTypeDependencies( type );
			  return selector.select( type, typeDependencies );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> Iterable<? extends T> resolveTypeDependencies(Class<T> type) throws IllegalArgumentException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public override IEnumerable<T> ResolveTypeDependencies<T>( Type type )
		 {
				 type = typeof( T );
			  return _life.LifecycleInstances.Where( type.isInstance ).Select( type.cast ).ToList();
		 }

		 private object GetKernelExtensionDependencies<T1>( KernelExtensionFactory<T1> factory )
		 {
			  Type configurationClass = ( Type )( ( ParameterizedType ) factory.GetType().GenericSuperclass ).ActualTypeArguments[0];
			  return DependenciesProxy.dependencies( _dependencies, configurationClass );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <T> org.neo4j.kernel.lifecycle.Lifecycle newInstance(org.neo4j.kernel.impl.spi.KernelContext kernelContext, KernelExtensionFactory<T> factory, Object dependencies)
		 private static Lifecycle NewInstance<T>( KernelContext kernelContext, KernelExtensionFactory<T> factory, object dependencies )
		 {
			  return factory.NewInstance( kernelContext, ( T )dependencies );
		 }
	}

}