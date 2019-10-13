﻿/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Api.Exceptions
{
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;

	/// <summary>
	/// This exception is thrown when committing an updating transaction in a read only database. Can also be thrown when
	/// trying to create tokens (like new property names) in a read only database.
	/// </summary>
	public class ReadOnlyDbException : TransactionFailureException
	{
		 public ReadOnlyDbException() : base(Status_General.ForbiddenOnReadOnlyDatabase, "This is a read only Neo4j instance")
		 {
		 }
	}

}