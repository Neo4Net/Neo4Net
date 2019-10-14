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
namespace Neo4Net.tools.txlog.checktypes
{
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;

	public class RelationshipCheckType : CheckType<Command.RelationshipCommand, RelationshipRecord>
	{
		 internal RelationshipCheckType() : base(typeof(Command.RelationshipCommand))
		 {
		 }

		 public override RelationshipRecord Before( Command.RelationshipCommand command )
		 {
			  return command.Before;
		 }

		 public override RelationshipRecord After( Command.RelationshipCommand command )
		 {
			  return command.After;
		 }

		 protected internal override bool InUseRecordsEqual( RelationshipRecord record1, RelationshipRecord record2 )
		 {
			  return record1.NextProp == record2.NextProp && record1.FirstInFirstChain == record2.FirstInFirstChain && record1.FirstInSecondChain == record2.FirstInSecondChain && record1.FirstNextRel == record2.FirstNextRel && record1.FirstNode == record2.FirstNode && record1.FirstPrevRel == record2.FirstPrevRel && record1.SecondNextRel == record2.SecondNextRel && record1.SecondNode == record2.SecondNode && record1.SecondPrevRel == record2.SecondPrevRel && record1.Type == record2.Type;
		 }

		 public override string Name()
		 {
			  return "relationship";
		 }
	}

}