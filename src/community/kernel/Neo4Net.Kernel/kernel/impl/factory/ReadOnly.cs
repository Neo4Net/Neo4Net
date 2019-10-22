/*
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
namespace Neo4Net.Kernel.impl.factory
{
	using WriteOperationsNotAllowedException = Neo4Net.GraphDb.security.WriteOperationsNotAllowedException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	public class ReadOnly : AccessCapability
	{
		 public override void AssertCanWrite()
		 {
			  throw new WriteOperationsNotAllowedException( "No write operations are allowed on this database. This is a read only Neo4Net instance.", Neo4Net.Kernel.Api.Exceptions.Status_General.ForbiddenOnReadOnlyDatabase );
		 }
	}

}