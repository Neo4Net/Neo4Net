using System;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using SchemaRead = Neo4Net.Kernel.Api.Internal.SchemaRead;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class SchemaIndexTestHelper
	{
		 private SchemaIndexTestHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.Neo4Net.kernel.extension.KernelExtensionFactory<SingleInstanceIndexProviderFactoryDependencies> singleInstanceIndexProviderFactory(String key, final org.Neo4Net.kernel.api.index.IndexProvider provider)
		 public static KernelExtensionFactory<SingleInstanceIndexProviderFactoryDependencies> SingleInstanceIndexProviderFactory( string key, IndexProvider provider )
		 {
			  return new SingleInstanceIndexProviderFactory( key, provider );
		 }

		 public interface ISingleInstanceIndexProviderFactoryDependencies
		 {
			  Config Config();
		 }

		 private class SingleInstanceIndexProviderFactory : KernelExtensionFactory<SingleInstanceIndexProviderFactoryDependencies>
		 {
			  internal readonly IndexProvider Provider;

			  internal SingleInstanceIndexProviderFactory( string key, IndexProvider provider ) : base( ExtensionType.DATABASE, key )
			  {
					this.Provider = provider;
			  }

			  public override Lifecycle NewInstance( KernelContext context, SingleInstanceIndexProviderFactoryDependencies dependencies )
			  {
					return Provider;
			  }
		 }

		 public static IndexProxy MockIndexProxy()
		 {
			  return mock( typeof( IndexProxy ) );
		 }

		 public static bool AwaitLatch( System.Threading.CountdownEvent latch )
		 {
			  try
			  {
					return latch.await( 10, SECONDS );
			  }
			  catch ( InterruptedException e )
			  {
					Thread.interrupted();
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void awaitIndexOnline(org.Neo4Net.Kernel.Api.Internal.SchemaRead schemaRead, org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public static void AwaitIndexOnline( SchemaRead schemaRead, IndexReference index )
		 {
			  long start = DateTimeHelper.CurrentUnixTimeMillis();
			  while ( schemaRead.IndexGetState( index ) != InternalIndexState.ONLINE )
			  {

					if ( start + 1000 * 10 < DateTimeHelper.CurrentUnixTimeMillis() )
					{
						 throw new Exception( "Index didn't come online within a reasonable time." );
					}
			  }
		 }
	}

}