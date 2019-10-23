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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;


	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.GraphDb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Neo4Net.GraphDb.factory.module.edition.CommunityEditionModule;
	using InvalidTransactionTypeKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using ImpermanentGraphDatabase = Neo4Net.Test.ImpermanentGraphDatabase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	public class KernelTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCreationOfConstraintsWhenInHA() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowCreationOfConstraintsWhenInHA()
		 {
			  //noinspection deprecation
			  GraphDatabaseAPI db = new FakeHaDatabase( this );
			  ThreadToStatementContextBridge stmtBridge = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );

			  using ( Transaction ignored = Db.beginTx() )
			  {
					KernelTransaction ktx = stmtBridge.GetKernelTransactionBoundToThisThread( true );
					try
					{
						 ktx.SchemaWrite().uniquePropertyConstraintCreate(forLabel(1, 1));
						 fail( "expected exception here" );
					}
					catch ( InvalidTransactionTypeKernelException e )
					{
						 assertThat( e.Message, containsString( "HA" ) );
					}
			  }

			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") class FakeHaDatabase extends org.Neo4Net.test.ImpermanentGraphDatabase
		 internal class FakeHaDatabase : ImpermanentGraphDatabase
		 {
			 private readonly KernelTest _outerInstance;

			 public FakeHaDatabase( KernelTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  protected internal override void Create( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
			  {
					System.Func<PlatformModule, AbstractEditionModule> factory = platformModule => new CommunityEditionModuleAnonymousInnerClass( this, platformModule );
					new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, factory, storeDir, dependencies )
					.initFacade( storeDir, @params, dependencies, this );
			  }

			  private class CommunityEditionModuleAnonymousInnerClass : CommunityEditionModule
			  {
				  private readonly FakeHaDatabase _outerInstance;

				  public CommunityEditionModuleAnonymousInnerClass( FakeHaDatabase outerInstance, UnknownType platformModule ) : base( platformModule )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override SchemaWriteGuard createSchemaWriteGuard()
				  {
						return () =>
						{
						 throw new InvalidTransactionTypeKernelException( "Creation or deletion of constraints is not possible while running in a HA cluster. " + "In order to do that, please restart in non-HA mode and propagate the database copy" + "to all slaves" );
						};
				  }
			  }

			  private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
			  {
				  private readonly FakeHaDatabase _outerInstance;

				  private File _storeDir;
				  private GraphDatabaseFacadeFactory.Dependencies _dependencies;

				  public GraphDatabaseFacadeFactoryAnonymousInnerClass( FakeHaDatabase outerInstance, DatabaseInfo community, System.Func<PlatformModule, AbstractEditionModule> factory, File storeDir, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, factory )
				  {
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._dependencies = dependencies;
				  }

				  protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
				  {
						return new ImpermanentPlatformModule( storeDir, config, databaseInfo, dependencies );
				  }
			  }
		 }
	}

}