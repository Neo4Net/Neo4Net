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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using KernelAPIWriteTestSupport = Neo4Net.@internal.Kernel.Api.KernelAPIWriteTestSupport;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using GraphDatabaseServiceCleaner = Neo4Net.Test.GraphDatabaseServiceCleaner;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

	internal class WriteTestSupport : KernelAPIWriteTestSupport
	{
		 private GraphDatabaseService _db;

		 public override void Setup( File storeDir )
		 {
			  _db = NewDb( storeDir );
		 }

		 protected internal virtual GraphDatabaseService NewDb( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder(storeDir).newGraphDatabase();
		 }

		 public override void ClearGraph()
		 {
			  GraphDatabaseServiceCleaner.cleanDatabaseContent( _db );
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

		 public override PropertyContainer GraphProperties()
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( EmbeddedProxySPI ) ).newGraphPropertiesProxy();
		 }

		 public override Kernel KernelToTest()
		 {
			  DependencyResolver resolver = ( ( GraphDatabaseAPI ) this._db ).DependencyResolver;
			  return resolver.ResolveDependency( typeof( Kernel ) );
		 }

		 public override GraphDatabaseService GraphBackdoor()
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