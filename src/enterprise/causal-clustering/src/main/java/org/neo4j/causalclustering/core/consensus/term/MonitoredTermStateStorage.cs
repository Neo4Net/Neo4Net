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
namespace Neo4Net.causalclustering.core.consensus.term
{

	using RaftTermMonitor = Neo4Net.causalclustering.core.consensus.log.monitoring.RaftTermMonitor;
	using Neo4Net.causalclustering.core.state.storage;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	public class MonitoredTermStateStorage : StateStorage<TermState>
	{
		 private string _termTag = "term";

		 private readonly StateStorage<TermState> @delegate;
		 private readonly RaftTermMonitor _termMonitor;

		 public MonitoredTermStateStorage( StateStorage<TermState> @delegate, Monitors monitors )
		 {
			  this.@delegate = @delegate;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  this._termMonitor = monitors.NewMonitor( typeof( RaftTermMonitor ), this.GetType().FullName, _termTag );
		 }

		 public virtual TermState InitialState
		 {
			 get
			 {
				  return @delegate.InitialState;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void persistStoreData(TermState state) throws java.io.IOException
		 public override void PersistStoreData( TermState state )
		 {
			  @delegate.PersistStoreData( state );
			  _termMonitor.term( state.CurrentTerm() );
		 }
	}

}