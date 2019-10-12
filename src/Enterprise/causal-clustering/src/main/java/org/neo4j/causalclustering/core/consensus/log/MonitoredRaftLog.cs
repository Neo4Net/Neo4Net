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
namespace Neo4Net.causalclustering.core.consensus.log
{

	using RaftLogAppendIndexMonitor = Neo4Net.causalclustering.core.consensus.log.monitoring.RaftLogAppendIndexMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	public class MonitoredRaftLog : DelegatingRaftLog
	{
		 private readonly RaftLogAppendIndexMonitor _appendIndexMonitor;

		 public MonitoredRaftLog( RaftLog @delegate, Monitors monitors ) : base( @delegate )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  this._appendIndexMonitor = monitors.NewMonitor( typeof( RaftLogAppendIndexMonitor ), this.GetType().FullName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long append(RaftLogEntry... entries) throws java.io.IOException
		 public override long Append( params RaftLogEntry[] entries )
		 {
			  long appendIndex = base.Append( entries );
			  _appendIndexMonitor.appendIndex( appendIndex );
			  return appendIndex;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(long fromIndex) throws java.io.IOException
		 public override void Truncate( long fromIndex )
		 {
			  base.Truncate( fromIndex );
			  _appendIndexMonitor.appendIndex( base.AppendIndex() );
		 }
	}

}