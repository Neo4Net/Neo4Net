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
namespace Neo4Net.Server.rest.web
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;

	//TODO: move this to another package. domain? or repr?
	public class PropertyValueException : BadInputException
	{
		 private const long SERIAL_VERSION_UID = -7810255514347322861L;

		 public PropertyValueException( string message ) : base( message )
		 {
		 }

		 public override Status Status()
		 {
			  return Neo4Net.Kernel.Api.Exceptions.Status_Statement.ArgumentError;
		 }
	}

}