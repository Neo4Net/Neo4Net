/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.backup.impl
{
	using Test = org.junit.Test;

	using PipelineWrapper = Neo4Net.causalclustering.handlers.PipelineWrapper;
	using SecureClientPipelineWrapper = Neo4Net.causalclustering.handlers.SecureClientPipelineWrapper;
	using RealOutsideWorld = Neo4Net.Commandline.Admin.RealOutsideWorld;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupSupportingClassesFactoryProvider.getProvidersByPriority;
	public class OnlineBackupCommandProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void communityBackupSupportingFactory()
		 public virtual void CommunityBackupSupportingFactory()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();

			  //OutsideWorld outsideWorld = mock( OutsideWorld.class );

			  RealOutsideWorld outsideWorld = new RealOutsideWorld();
			  FileSystemAbstraction fileSystemMock = mock( typeof( FileSystemAbstraction ) );

			  //outsideWorld.fileSystemAbstraction = fileSystemMock;
			  Monitors monitors = mock( typeof( Monitors ) );

			  BackupModule backupModule = new BackupModule( outsideWorld, logProvider, monitors );

			  // when( backupModule.getOutsideWorld() ).thenReturn( outsideWorld );

			  BackupSupportingClassesFactoryProvider provider = ProvidersByPriority.findFirst().get();

			  BackupSupportingClassesFactory factory = provider.GetFactory( backupModule );

			  /*
			  SecurePipelineWrapperFactory pipelineWrapperFactory = new SecurePipelineWrapperFactory();
			  SslPolicyLoader sslPolicyLoader;
			  // and
			  Config config = Config.defaults();
			  config.augment( CausalClusteringSettings.ssl_policy, "default" );
	
			  // We want to create dependencies the same way factory.createPipelineWrapper does so.s
			  Dependencies dependencies = new Dependencies(  );
			  dependencies.satisfyDependencies(new Object[]{SslPolicyLoader.create(config, logProvider)});
	
			  assertEquals( pipelineWrapperFactory.forClient(config, dependencies, logProvider, CausalClusteringSettings.ssl_policy),
			          factory.createPipelineWrapper( Config.defaults() ) );
			  */

			  assertEquals( typeof( SecureClientPipelineWrapper ), factory.CreatePipelineWrapper( Config.defaults() ).GetType() );
		 }

		 /// <summary>
		 /// This class must be public and static because it must be service loadable.
		 /// </summary>
		 public class DummyProvider : BackupSupportingClassesFactoryProvider
		 {
			  public override BackupSupportingClassesFactory GetFactory( BackupModule backupModule )
			  {
					return new BackupSupportingClassesFactoryAnonymousInnerClass( this, backupModule );
			  }

			  private class BackupSupportingClassesFactoryAnonymousInnerClass : BackupSupportingClassesFactory
			  {
				  private readonly DummyProvider _outerInstance;

				  public BackupSupportingClassesFactoryAnonymousInnerClass( DummyProvider outerInstance, Neo4Net.backup.impl.BackupModule backupModule ) : base( backupModule )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override PipelineWrapper createPipelineWrapper( Config config )
				  {
						throw new AssertionError( "This provider should never be loaded" );
				  }
			  }

			  protected internal override int Priority
			  {
				  get
				  {
						return base.Priority - 1;
				  }
			  }
		 }
	}

}