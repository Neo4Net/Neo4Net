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
namespace Neo4Net.Kernel.impl.transaction.state
{

	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using Neo4Net.Kernel.Api.Index;
	using Neo4Net.Kernel.Impl.Api.index;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;

	/// <summary>
	/// Provides direct access to updates.
	/// </summary>
	public class DirectIndexUpdates : IndexUpdates
	{
		 private readonly IEnumerable<IndexEntryUpdate<SchemaDescriptor>> _updates;

		 public DirectIndexUpdates( IEnumerable<IndexEntryUpdate<SchemaDescriptor>> updates )
		 {
			  this._updates = updates;
		 }

		 public override IEnumerator<IndexEntryUpdate<SchemaDescriptor>> Iterator()
		 {
			  return _updates.GetEnumerator();
		 }

		 public override void Feed( IEntityCommandGrouper<Command.NodeCommand>.Cursor nodeCommands, IEntityCommandGrouper<Command.RelationshipCommand>.Cursor relationshipCommands )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool HasUpdates()
		 {
			  return true;
		 }
	}

}