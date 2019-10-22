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
namespace Neo4Net.Kernel.impl.locking
{
	using Description = Neo4Net.Configuration.Description;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.GraphDb.config;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.setting;

	/// <summary>
	/// A <seealso cref="StatementLocksFactory"/> that created <seealso cref="DeferringStatementLocks"/> based on the given
	/// <seealso cref="Locks"/> and <seealso cref="Config"/>.
	/// </summary>
	public class DeferringStatementLocksFactory : StatementLocksFactory, LoadableConfig
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Enable deferring of locks to commit time. This feature weakens the isolation level. " + "It can result in both domain and storage level inconsistencies.") public static final org.Neo4Net.graphdb.config.Setting<bool> deferred_locks_enabled = setting("unsupported.dbms.deferred_locks.enabled", org.Neo4Net.kernel.configuration.Settings.BOOLEAN, org.Neo4Net.kernel.configuration.Settings.FALSE);
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