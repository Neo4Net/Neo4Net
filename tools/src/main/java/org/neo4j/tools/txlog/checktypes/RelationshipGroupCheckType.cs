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
namespace Org.Neo4j.tools.txlog.checktypes
{
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;

	public class RelationshipGroupCheckType : CheckType<Command.RelationshipGroupCommand, RelationshipGroupRecord>
	{
		 internal RelationshipGroupCheckType() : base(typeof(Command.RelationshipGroupCommand))
		 {
		 }

		 public override RelationshipGroupRecord Before( Command.RelationshipGroupCommand command )
		 {
			  return command.Before;
		 }

		 public override RelationshipGroupRecord After( Command.RelationshipGroupCommand command )
		 {
			  return command.After;
		 }

		 protected internal override bool InUseRecordsEqual( RelationshipGroupRecord record1, RelationshipGroupRecord record2 )
		 {
			  return record1.FirstIn == record2.FirstIn && record1.FirstLoop == record2.FirstLoop && record1.FirstOut == record2.FirstOut && record1.Next == record2.Next && record1.OwningNode == record2.OwningNode && record1.Prev == record2.Prev && record1.Type == record2.Type;
		 }

		 public override string Name()
		 {
			  return "relationship_group";
		 }
	}

}