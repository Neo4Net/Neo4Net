using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Index.lucene
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using IndexProviders = Neo4Net.Kernel.spi.legacyindex.IndexProviders;

	/// @deprecated removed in 4.0 
	[Obsolete("removed in 4.0")]
	public class LuceneKernelExtension : LifecycleAdapter
	{
		 private readonly Neo4Net.Kernel.Api.Impl.Index.LuceneKernelExtension @delegate;

		 /// @deprecated removed in 4.0 
		 [Obsolete("removed in 4.0")]
		 public LuceneKernelExtension( File storeDir, Config config, System.Func<IndexConfigStore> indexStore, FileSystemAbstraction fileSystemAbstraction, IndexProviders indexProviders ) : this( storeDir, config, indexStore, fileSystemAbstraction, indexProviders, OperationalMode.single )
		 {
		 }

		 /// @deprecated removed in 4.0 
		 [Obsolete("removed in 4.0")]
		 public LuceneKernelExtension( File storeDir, Config config, System.Func<IndexConfigStore> indexStore, FileSystemAbstraction fileSystemAbstraction, IndexProviders indexProviders, OperationalMode operationalMode )
		 {
			  Neo4Net.Kernel.spi.explicitindex.IndexProviders proxyIndexProviders = CreateImposterOf( typeof( Neo4Net.Kernel.spi.explicitindex.IndexProviders ), indexProviders );
			  @delegate = new Neo4Net.Kernel.Api.Impl.Index.LuceneKernelExtension( storeDir, config, indexStore, fileSystemAbstraction, proxyIndexProviders, operationalMode );
		 }

		 public override void Init()
		 {
			  @delegate.Init();
		 }

		 public override void Shutdown()
		 {
			  @delegate.Shutdown();
		 }

		 /// <summary>
		 /// Create an imposter of an interface. This is effectively used to mimic duck-typing.
		 /// </summary>
		 /// <param name="target"> the interface to mimic. </param>
		 /// <param name="imposter"> the instance of any class, it has to implement all methods of the interface provided by {@code target}. </param>
		 /// @param <T> the type of interface to mimic. </param>
		 /// @param <F> the actual type of the imposter. </param>
		 /// <returns> an imposter that can be passed as the type of mimicked interface.
		 /// 
		 /// @implNote Method conformity is never checked, this is up to the user of the function to ensure. Sharp tool, use
		 /// with caution. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <T,F> T createImposterOf(Class<T> target, F imposter)
		 private static T CreateImposterOf<T, F>( Type target, F imposter )
		 {
				 target = typeof( T );
			  return ( T ) Proxy.newProxyInstance( target.ClassLoader, new Type[]{ target }, new MirroredInvocationHandler<>( imposter ) );
		 }

		 /// <summary>
		 /// Will pass through everything, as is, to the wrapped instance.
		 /// </summary>
		 private class MirroredInvocationHandler<F> : InvocationHandler
		 {
			  internal readonly F Wrapped;

			  internal MirroredInvocationHandler( F wrapped )
			  {
					this.Wrapped = wrapped;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object invoke(Object proxy, Method method, Object[] args) throws Throwable
			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					System.Reflection.MethodInfo match = Wrapped.GetType().GetMethod(method.Name, method.ParameterTypes);
					return match.invoke( Wrapped, args );
			  }
		 }
	}

}