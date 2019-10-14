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
namespace Neo4Net.Kernel.impl.query
{
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;

	public interface QueryLogger
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 QueryLogger NO_LOG = new QueryLogger()
	//	 {
	//		  @@Override public void success(ExecutingQuery query)
	//		  {
	//		  }
	//
	//		  @@Override public void failure(ExecutingQuery query, Throwable throwable)
	//		  {
	//		  }
	//	 };

		 void Success( ExecutingQuery query );
		 void Failure( ExecutingQuery query, Exception throwable );
	}

}