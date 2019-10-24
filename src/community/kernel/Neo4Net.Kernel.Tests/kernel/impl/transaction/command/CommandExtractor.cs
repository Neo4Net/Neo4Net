using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using Neo4Net.Collections.Helpers;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

	public class CommandExtractor : Visitor<StorageCommand, IOException>
	{
		 private readonly IList<StorageCommand> _commands = new List<StorageCommand>();

		 public override bool Visit( StorageCommand element )
		 {
			  _commands.Add( element );
			  return false;
		 }

		 public virtual IList<StorageCommand> Commands
		 {
			 get
			 {
				  return _commands;
			 }
		 }
	}

}