/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.Impl.Api
{

	using QueryRegistrationOperations = Neo4Net.Kernel.Impl.Api.operations.QueryRegistrationOperations;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ObjectUtils.firstNonNull;

	public class StatementOperationParts
	{
		 private readonly QueryRegistrationOperations _queryRegistrationOperations;
		 private static readonly string _errorMsg = "No part of type " + typeof( QueryRegistrationOperations ).Name + " assigned";

		 public StatementOperationParts( QueryRegistrationOperations queryRegistrationOperations )
		 {
			  this._queryRegistrationOperations = queryRegistrationOperations;
		 }

		 internal virtual QueryRegistrationOperations QueryRegistrationOperations()
		 {
			  return Objects.requireNonNull( _queryRegistrationOperations, _errorMsg );
		 }

		 public virtual StatementOperationParts Override( QueryRegistrationOperations queryRegistrationOperations )
		 {
			  return new StatementOperationParts( firstNonNull( queryRegistrationOperations, this._queryRegistrationOperations ) );
		 }
	}

}