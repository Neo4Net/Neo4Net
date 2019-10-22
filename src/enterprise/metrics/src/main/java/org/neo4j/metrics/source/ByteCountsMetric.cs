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
namespace Neo4Net.metrics.source
{
	using Counter = com.codahale.metrics.Counter;

	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;

	public class ByteCountsMetric : ByteCounterMonitor
	{
		 private readonly Counter _bytesWritten = new Counter();
		 private readonly Counter _bytesRead = new Counter();

		 public virtual long BytesWritten
		 {
			 get
			 {
				  return _bytesWritten.Count;
			 }
		 }

		 public virtual long BytesRead
		 {
			 get
			 {
				  return _bytesRead.Count;
			 }
		 }

		 public override void BytesWritten( long bytes )
		 {
			  _bytesWritten.inc( bytes );
		 }

		 public override void BytesRead( long bytes )
		 {
			  _bytesRead.inc( bytes );
		 }
	}

}