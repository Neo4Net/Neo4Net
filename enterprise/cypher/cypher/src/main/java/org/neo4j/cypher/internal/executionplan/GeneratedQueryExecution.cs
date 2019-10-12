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
namespace Org.Neo4j.Cypher.@internal.executionplan
{
	using ExecutionMode = Org.Neo4j.Cypher.@internal.runtime.ExecutionMode;
	using InternalPlanDescription = Org.Neo4j.Cypher.@internal.runtime.planDescription.InternalPlanDescription;
	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;

	public interface GeneratedQueryExecution
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <E extends Exception> void accept(org.neo4j.cypher.result.QueryResult_QueryResultVisitor<E> visitor) throws E;
		 void accept<E>( Org.Neo4j.Cypher.result.QueryResult_QueryResultVisitor<E> visitor );

		 ExecutionMode ExecutionMode();

		 InternalPlanDescription ExecutionPlanDescription();

		 string[] FieldNames();
	}

}