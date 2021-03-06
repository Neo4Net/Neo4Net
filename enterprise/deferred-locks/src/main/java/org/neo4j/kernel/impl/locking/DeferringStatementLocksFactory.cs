﻿/*
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
namespace Org.Neo4j.Kernel.impl.locking
{
	using Description = Org.Neo4j.Configuration.Description;
	using Internal = Org.Neo4j.Configuration.Internal;
	using LoadableConfig = Org.Neo4j.Configuration.LoadableConfig;
	using Org.Neo4j.Graphdb.config;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	/// <summary>
	/// A <seealso cref="StatementLocksFactory"/> that created <seealso cref="DeferringStatementLocks"/> based on the given
	/// <seealso cref="Locks"/> and <seealso cref="Config"/>.
	/// </summary>
	public class DeferringStatementLocksFactory : StatementLocksFactory, LoadableConfig
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Enable deferring of locks to commit time. This feature weakens the isolation level. " + "It can result in both domain and storage level inconsistencies.") public static final org.neo4j.graphdb.config.Setting<bool> deferred_locks_enabled = setting("unsupported.dbms.deferred_locks.enabled", org.neo4j.kernel.configuration.Settings.BOOLEAN, org.neo4j.kernel.configuration.Settings.FALSE);
		 [Description("Enable deferring of locks to commit time. This feature weakens the isolation level. " + "It can result in both domain and storage level inconsistencies.")]
		 public static readonly Setting<bool> DeferredLocksEnabled = setting( "unsupported.dbms.deferred_locks.enabled", Settings.BOOLEAN, Settings.FALSE );

		 private Locks _locks;
		 private bool _deferredLocksEnabled;

		 public override void Initialize( Locks locks, Config config )
		 {
			  this._locks = requireNonNull( locks );
			  this._deferredLocksEnabled = config.Get( DeferredLocksEnabled );
		 }

		 public override StatementLocks NewInstance()
		 {
			  if ( _locks == null )
			  {
					throw new System.InvalidOperationException( "Factory has not been initialized" );
			  }

			  Locks_Client client = _locks.newClient();
			  return _deferredLocksEnabled ? new DeferringStatementLocks( client ) : new SimpleStatementLocks( client );
		 }
	}

}