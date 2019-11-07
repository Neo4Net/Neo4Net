/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactoryState = Neo4Net.GraphDb.factory.GraphDatabaseFactoryState;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.GraphDb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HighlyAvailableEditionModule = Neo4Net.Kernel.ha.factory.HighlyAvailableEditionModule;
	using TransactionVersionContextSupplier = Neo4Net.Kernel.impl.context.TransactionVersionContextSupplier;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.TRUE;

	public class UpdatePullerTriggersPageTransactionTrackingIT
	{
		private bool InstanceFieldsInitialized = false;

		public UpdatePullerTriggersPageTransactionTrackingIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_contextSupplier = new TestTransactionVersionContextSupplier( this );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.ha.ClusterRule clusterRule = new Neo4Net.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();
		 private readonly Label _nodeLabel = Label.label( "mark" );
		 private TestTransactionVersionContextSupplier _contextSupplier;
		 private ClusterManager.ManagedCluster _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  CustomGraphDatabaseFactory customGraphDatabaseFactory = new CustomGraphDatabaseFactory( this );
			  _cluster = ClusterRule.withSharedSetting( GraphDatabaseSettings.snapshot_query, TRUE ).withDbFactory( customGraphDatabaseFactory ).startCluster();
			  HighlyAvailableGraphDatabase master = _cluster.Master;
			  for ( int i = 0; i < 3; i++ )
			  {
					using ( Transaction tx = master.BeginTx() )
					{
						 master.CreateNode( _nodeLabel );
						 tx.Success();
					}
			  }
			  _cluster.sync();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updatePullerTriggerPageTransactionTracking()
		 public virtual void UpdatePullerTriggerPageTransactionTracking()
		 {
			  HighlyAvailableGraphDatabase slave = _cluster.AnySlave;
			  TransactionIdStore slaveTransactionIdStore = slave.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
			  assertEquals( 5, slaveTransactionIdStore.LastClosedTransactionId );

			  ByzantineLongSupplier byzantineIdSupplier = _contextSupplier.ByzantineIdSupplier;
			  byzantineIdSupplier.UseWrongTxId();
			  try
			  {
					  using ( Transaction ignored = slave.BeginTx() )
					  {
						slave.Execute( "match (n) return n" );
					  }
			  }
			  catch ( QueryExecutionException executionException )
			  {
					assertEquals( "Unable to get clean data snapshot for query 'match (n) return n' after 5 attempts.", executionException.Message );
			  }
			  byzantineIdSupplier.UseCorrectTxId();
			  slave.Execute( "match (n) return n" ).close();
		 }

		 private class CustomGraphDatabaseFactory : TestHighlyAvailableGraphDatabaseFactory
		 {
			 private readonly UpdatePullerTriggersPageTransactionTrackingIT _outerInstance;

			 public CustomGraphDatabaseFactory( UpdatePullerTriggersPageTransactionTrackingIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  protected internal override GraphDatabaseBuilder.DatabaseCreator CreateDatabaseCreator( File storeDir, GraphDatabaseFactoryState state )
			  {
					return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
			  }

			  private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
			  {
				  private readonly CustomGraphDatabaseFactory _outerInstance;

				  private File _storeDir;
				  private GraphDatabaseFactoryState _state;

				  public DatabaseCreatorAnonymousInnerClass( CustomGraphDatabaseFactory outerInstance, File storeDir, GraphDatabaseFactoryState state )
				  {
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._state = state;
				  }

				  public IGraphDatabaseService newDatabase( Config config )
				  {
						config.Augment( stringMap( "unsupported.dbms.ephemeral", "false" ) );
						return new CustomHighlyAvailableGraphDatabase( _outerInstance.outerInstance, _storeDir, config, _state.databaseDependencies() );
				  }
			  }
		 }

		 private class CustomHighlyAvailableGraphDatabase : HighlyAvailableGraphDatabase
		 {
			 private readonly UpdatePullerTriggersPageTransactionTrackingIT _outerInstance;


			  internal CustomHighlyAvailableGraphDatabase( UpdatePullerTriggersPageTransactionTrackingIT outerInstance, File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, dependencies )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override GraphDatabaseFacadeFactory NewHighlyAvailableFacadeFactory()
			  {
					return new CustomFacadeFactory( _outerInstance, this );
			  }
		 }

		 private class CustomFacadeFactory : GraphDatabaseFacadeFactory
		 {
			 private readonly UpdatePullerTriggersPageTransactionTrackingIT _outerInstance;

			  internal CustomFacadeFactory( UpdatePullerTriggersPageTransactionTrackingIT outerInstance, CustomHighlyAvailableGraphDatabase customHighlyAvailableGraphDatabase ) : base(DatabaseInfo.HA, platformModule ->
			  {
			  this._outerInstance = outerInstance;
			  {
					 customHighlyAvailableGraphDatabase.Module = new HighlyAvailableEditionModule( platformModule );
					 return customHighlyAvailableGraphDatabase.Module;
			  });
			  }

			  public override GraphDatabaseFacade NewFacade( File storeDir, Config config, Dependencies dependencies )
			  {
					return InitFacade( storeDir, config, dependencies, new HighlyAvailableGraphDatabase( storeDir, config, dependencies ) );
			  }

			  protected internal override PlatformModule CreatePlatform( File storeDir, Config config, Dependencies dependencies )
			  {
					return new PlatformModuleAnonymousInnerClass( this, storeDir, config, DatabaseInfo, dependencies );
			  }

			  private class PlatformModuleAnonymousInnerClass : PlatformModule
			  {
				  private readonly CustomFacadeFactory _outerInstance;

				  private new Config _config;

				  public PlatformModuleAnonymousInnerClass( CustomFacadeFactory outerInstance, File storeDir, Config config, DatabaseInfo databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
				  {
					  this.outerInstance = outerInstance;
					  this._config = config;
				  }

				  protected internal override VersionContextSupplier createCursorContextSupplier( Config config )
				  {
						return outerInstance.outerInstance.contextSupplier;
				  }
			  }
		 }

		 private class TestTransactionVersionContextSupplier : TransactionVersionContextSupplier
		 {
			 private readonly UpdatePullerTriggersPageTransactionTrackingIT _outerInstance;

			 public TestTransactionVersionContextSupplier( UpdatePullerTriggersPageTransactionTrackingIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  internal volatile ByzantineLongSupplier ByzantineLongSupplier;

			  public override void Init( System.Func<long> lastClosedTransactionIdSupplier )
			  {
					ByzantineLongSupplier = new ByzantineLongSupplier( _outerInstance, lastClosedTransactionIdSupplier );
					base.Init( ByzantineLongSupplier );
			  }

			  public override VersionContext VersionContext
			  {
				  get
				  {
						return base.VersionContext;
				  }
			  }

			  internal virtual ByzantineLongSupplier ByzantineIdSupplier
			  {
				  get
				  {
						return ByzantineLongSupplier;
				  }
			  }
		 }

		 private class ByzantineLongSupplier : System.Func<long>
		 {
			 private readonly UpdatePullerTriggersPageTransactionTrackingIT _outerInstance;


			  internal volatile bool WrongTxId;
			  internal readonly System.Func<long> OriginalIdSupplier;

			  internal ByzantineLongSupplier( UpdatePullerTriggersPageTransactionTrackingIT outerInstance, System.Func<long> originalIdSupplier )
			  {
				  this._outerInstance = outerInstance;
					this.OriginalIdSupplier = originalIdSupplier;
			  }

			  public override long AsLong
			  {
				  get
				  {
						return WrongTxId ? 1 : OriginalIdSupplier.AsLong;
				  }
			  }

			  internal virtual void UseWrongTxId()
			  {
					WrongTxId = true;
			  }

			  internal virtual void UseCorrectTxId()
			  {
					WrongTxId = false;
			  }
		 }
	}

}