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
namespace Neo4Net.causalclustering.core.replication
{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class BenchmarkResult
	public class BenchmarkResult
	{
		 public long? TotalRequests;
		 public long? TotalBytes;
		 public long? TimeMillis;
		 public double? OpsPerMilli;
		 public double? MbPerSecond;

		 internal BenchmarkResult( long totalRequests, long totalBytes, long totalTimeMillis )
		 {
			  this.TotalRequests = totalRequests;
			  this.TotalBytes = totalBytes;
			  this.TimeMillis = totalTimeMillis;
			  this.OpsPerMilli = totalRequests / ( double ) totalTimeMillis;
			  this.MbPerSecond = totalBytes / ( double ) totalTimeMillis / 1048576 * 1000;
		 }
	}

}