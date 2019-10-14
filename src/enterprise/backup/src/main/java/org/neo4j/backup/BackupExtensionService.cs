using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.backup
{

	using Args = Neo4Net.Helpers.Args;
	using Service = Neo4Net.Helpers.Service;
	using LogService = Neo4Net.Logging.Internal.LogService;

	/// <summary>
	/// <para>
	/// This class provides a basic interface for backup sources to implement their
	/// own resolution algorithms. The backup tool in general expects a location to
	/// backup from but the format of it is in general specific to the source
	/// database, while the OnlineBackup class expects a valid socket to connect to
	/// and perform the backup. For that reason each implementation is expected to
	/// provide a translator from its specific addressing scheme to a valid
	/// <i>host:port</i> combination.
	/// </para>
	/// <para>
	/// The prime consumer of this API is the HA component, where a set of cluster
	/// members can be passed as targets to backup but only one will be used. It is
	/// expected therefore that a <seealso cref="Service"/> implementation will be present on
	/// the classpath that will properly communicate with the cluster and find the
	/// master.
	/// </para>
	/// <para>
	/// The URI is strictly expected to have a scheme component, matching the name of
	/// the service implementation used to resolve it. The same holds for the default
	/// case, with a scheme name of "single". The scheme specific fragment after that
	/// will be the responsibility of the plugin to resolve to a valid host. In any
	/// case, the resolve method is expected to return a valid URI, with a scheme
	/// which is the same as the one passed to it (ie the service's name).
	/// </para> </summary>
	/// @deprecated This will move to an internal package in the future. 
	[Obsolete("This will move to an internal package in the future.")]
	public abstract class BackupExtensionService : Service
	{
		 public BackupExtensionService( string name ) : base( name )
		 {
		 }

		 /// <summary>
		 /// The source specific target to valid backup host translation method.
		 /// </summary>
		 /// <param name="address"> Cluster address as passed in the command line </param>
		 /// <param name="arguments"> all arguments to the backup command </param>
		 /// <param name="logService"> the logging service to use </param>
		 /// <returns> A URI where the scheme is the service's name and there exist host
		 ///         and port parts that point to a backup source. </returns>
		 public abstract URI Resolve( string address, Args arguments, LogService logService );
	}

}