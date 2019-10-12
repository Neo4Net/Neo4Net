using System.Text;

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
namespace Neo4Net.Kernel.ha
{
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using DiagnosticsPhase = Neo4Net.@internal.Diagnostics.DiagnosticsPhase;
	using DiagnosticsProvider = Neo4Net.@internal.Diagnostics.DiagnosticsProvider;
	using HighAvailabilityMemberStateMachine = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using Logger = Neo4Net.Logging.Logger;

	/// <summary>
	/// TODO
	/// </summary>
	public class HighAvailabilityDiagnostics : DiagnosticsProvider
	{
		 private readonly HighAvailabilityMemberStateMachine _memberStateMachine;
		 private readonly ClusterClient _clusterClient;

		 public HighAvailabilityDiagnostics( HighAvailabilityMemberStateMachine memberStateMachine, ClusterClient clusterClient )
		 {
			  this._memberStateMachine = memberStateMachine;
			  this._clusterClient = clusterClient;
		 }

		 public virtual string DiagnosticsIdentifier
		 {
			 get
			 {
				  return this.GetType().Name;
			 }
		 }

		 public override void AcceptDiagnosticsVisitor( object visitor )
		 {
		 }

		 public override void Dump( DiagnosticsPhase phase, Logger logger )
		 {
			  StringBuilder builder = new StringBuilder();

			  builder.Append( "High Availability diagnostics\n" ).Append( "Member state:" ).Append( _memberStateMachine.CurrentState.name() ).Append("\n").Append("State machines:\n");

			  _clusterClient.dumpDiagnostics( builder );
			  logger.Log( builder.ToString() );
		 }
	}

}