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
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;

	public class NeoStoreCheckType : CheckType<Command.NeoStoreCommand, NeoStoreRecord>
	{
		 internal NeoStoreCheckType() : base(typeof(Command.NeoStoreCommand))
		 {
		 }

		 public override NeoStoreRecord Before( Command.NeoStoreCommand command )
		 {
			  return command.Before;
		 }

		 public override NeoStoreRecord After( Command.NeoStoreCommand command )
		 {
			  return command.After;
		 }

		 protected internal override bool InUseRecordsEqual( NeoStoreRecord record1, NeoStoreRecord record2 )
		 {
			  return record1.NextProp == record2.NextProp;
		 }

		 public override string Name()
		 {
			  return "neo_store";
		 }
	}

}