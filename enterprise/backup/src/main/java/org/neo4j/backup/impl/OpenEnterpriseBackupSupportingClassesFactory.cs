/*
 * Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */

namespace Org.Neo4j.backup.impl
{
	using PipelineWrapper = Org.Neo4j.causalclustering.handlers.PipelineWrapper;
	using SecurePipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.SecurePipelineWrapperFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;

	/// <summary>
	/// This file allows us to create our on SecurePipelineWrapperFactory object.
	/// The default BackupSupportingClassesFactory.createPipelineWrapper method returns a VoidPipelineWrapperFactory
	/// We simply create a SecurePipelineWrapper
	/// </summary>
	public class OpenEnterpriseBackupSupportingClassesFactory : BackupSupportingClassesFactory
	{
		 internal OpenEnterpriseBackupSupportingClassesFactory( BackupModule backupModule ) : base( backupModule )
		 {
		 }

		 /// <param name="config">
		 /// @return </param>
		 protected internal virtual PipelineWrapper CreatePipelineWrapper( Config config )
		 {
			  SecurePipelineWrapperFactory factory = new SecurePipelineWrapperFactory();
			  Dependencies deps = new Dependencies();
			  deps.SatisfyDependencies( new object[]{ SslPolicyLoader.create( config, this.LogProvider ) } );
			  return factory.ForClient( config, deps, this.LogProvider, OnlineBackupSettings.ssl_policy );
		 }
	}


}