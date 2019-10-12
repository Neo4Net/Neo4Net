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
namespace Neo4Net.tools.txlog.checktypes
{
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;

	internal class NodeCheckType : CheckType<Command.NodeCommand, NodeRecord>
	{
		 internal NodeCheckType() : base(typeof(Command.NodeCommand))
		 {
		 }

		 public override NodeRecord Before( Command.NodeCommand command )
		 {
			  return command.Before;
		 }

		 public override NodeRecord After( Command.NodeCommand command )
		 {
			  return command.After;
		 }

		 protected internal override bool InUseRecordsEqual( NodeRecord record1, NodeRecord record2 )
		 {
			  return record1.NextProp == record2.NextProp && record1.NextRel == record2.NextRel && record1.Dense == record2.Dense && record1.LabelField == record2.LabelField;
		 }

		 public override string Name()
		 {
			  return "node";
		 }
	}

}