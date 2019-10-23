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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using KernelAPIWriteTestSupport = Neo4Net.Kernel.Api.Internal.KernelAPIWriteTestSupport;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using IGraphDatabaseServiceCleaner = Neo4Net.Test.GraphDatabaseServiceCleaner;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

	internal class WriteTestSupport : KernelAPIWriteTestSupport
	{
		 private IGraphDatabaseService _db;

		 public override void Setup( File storeDir )
		 {
			  _db = NewDb( storeDir );
		 }

		 protected internal virtual IGraphDatabaseService NewDb( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder(storeDir).newGraphDatabase();
		 }

		 public override void ClearGraph()
		 {
			  IGraphDatabaseServiceCleaner.cleanDatabaseContent( _db );
			  using ( Transaction tx = _db.beginTx() )
			  {
					PropertyContainer graphProperties = graphProperties();
					foreach ( string key in graphProperties.PropertyKeys )
					{
						 graphProperties.RemoveProperty( key );
					}
					tx.Success();
			  }
		 }

		 public override IPropertyContainer GraphProperties()
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( EmbeddedProxySPI ) ).newGraphPropertiesProxy();
		 }

		 public override Kernel KernelToTest()
		 {
			  DependencyResolver resolver = ( ( GraphDatabaseAPI ) this._db ).DependencyResolver;
			  return resolver.ResolveDependency( typeof( Kernel ) );
		 }

		 public override IGraphDatabaseService GraphBackdoor()
		 {
			  return _db;
		 }

		 public override void TearDown()
		 {
			  _db.shutdown();
			  _db = null;
		 }
	}

}