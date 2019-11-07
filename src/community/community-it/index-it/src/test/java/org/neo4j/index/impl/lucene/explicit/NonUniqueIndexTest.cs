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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseFactoryState = Neo4Net.GraphDb.factory.GraphDatabaseFactoryState;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using CommunityEditionModule = Neo4Net.GraphDb.factory.module.edition.CommunityEditionModule;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using NodeValueIndexCursor = Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using CentralJobScheduler = Neo4Net.Kernel.impl.scheduler.CentralJobScheduler;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;

	public class NonUniqueIndexTest
	{
		 private const string LABEL = "SomeLabel";
		 private const string KEY = "key";
		 private const string VALUE = "value";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.PageCacheAndDependenciesRule resources = new Neo4Net.test.rule.PageCacheAndDependenciesRule().with(new Neo4Net.test.rule.fs.DefaultFileSystemRule());
		 public PageCacheAndDependenciesRule Resources = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentIndexPopulationAndInsertsShouldNotProduceDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentIndexPopulationAndInsertsShouldNotProduceDuplicates()
		 {
			  // Given
			  Config config = Config.defaults();
			  IGraphDatabaseService db = NewEmbeddedGraphDatabaseWithSlowJobScheduler( config );
			  try
			  {
					// When
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().indexFor(label(LABEL)).on(KEY).create();
						 tx.Success();
					}
					Node node;
					using ( Transaction tx = Db.beginTx() )
					{
						 node = Db.createNode( label( LABEL ) );
						 node.SetProperty( KEY, VALUE );
						 tx.Success();
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().awaitIndexesOnline(1, MINUTES);
						 tx.Success();
					}

					// Then
					using ( Transaction tx = Db.beginTx() )
					{
						 KernelTransaction ktx = ( ( GraphDatabaseAPI ) db ).DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
						 IndexReference index = ktx.SchemaRead().index(ktx.TokenRead().nodeLabel(LABEL), ktx.TokenRead().propertyKey(KEY));
						 NodeValueIndexCursor cursor = ktx.Cursors().allocateNodeValueIndexCursor();
						 ktx.DataRead().nodeIndexSeek(index, cursor, IndexOrder.NONE, false, IndexQuery.exact(1, VALUE));
						 assertTrue( cursor.Next() );
						 assertEquals( node.Id, cursor.NodeReference() );
						 assertFalse( cursor.Next() );
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private IGraphDatabaseService NewEmbeddedGraphDatabaseWithSlowJobScheduler( Config config )
		 {
			  GraphDatabaseFactoryState graphDatabaseFactoryState = new GraphDatabaseFactoryState();
			  graphDatabaseFactoryState.UserLogProvider = NullLogService.Instance.UserLogProvider;
			  return new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, config )
			  .newFacade( Resources.directory().storeDir(), config, graphDatabaseFactoryState.DatabaseDependencies() );
		 }

		 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
		 {
			 private readonly NonUniqueIndexTest _outerInstance;

			 private Config _config;

			 public GraphDatabaseFacadeFactoryAnonymousInnerClass( NonUniqueIndexTest outerInstance, DatabaseInfo community, Config config ) : base( community, CommunityEditionModule::new )
			 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				 this.outerInstance = outerInstance;
				 this._config = config;
			 }

			 protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
			 {
				  return new PlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies );
			 }

			 private class PlatformModuleAnonymousInnerClass : PlatformModule
			 {
				 private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

				 public PlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File storeDir, Config config, UnknownType databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
				 {
					 this.outerInstance = outerInstance;
				 }

				 protected internal override CentralJobScheduler createJobScheduler()
				 {
					  return NewSlowJobScheduler();
				 }

				 protected internal override LogService createLogService( LogProvider userLogProvider )
				 {
					  return NullLogService.Instance;
				 }
			 }
		 }

		 private static CentralJobScheduler NewSlowJobScheduler()
		 {
			  return new CentralJobSchedulerAnonymousInnerClass();
		 }

		 private class CentralJobSchedulerAnonymousInnerClass : CentralJobScheduler
		 {
			 public override JobHandle schedule( Group group, ThreadStart job )
			 {
				  return base.schedule( group, SlowRunnable( job ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Runnable slowRunnable(final Runnable target)
		 private static ThreadStart SlowRunnable( ThreadStart target )
		 {
			  return () =>
			  {
				LockSupport.parkNanos( 100_000_000L );
				target.run();
			  };
		 }
	}

}