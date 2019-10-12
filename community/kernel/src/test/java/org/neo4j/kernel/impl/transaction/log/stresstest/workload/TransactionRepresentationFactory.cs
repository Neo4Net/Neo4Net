using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.transaction.log.stresstest.workload
{


	using TransactionHeaderInformation = Org.Neo4j.Kernel.Impl.Api.TransactionHeaderInformation;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.TransactionHeaderInformationFactory.DEFAULT;

	internal class TransactionRepresentationFactory
	{
		 private readonly CommandGenerator _commandGenerator = new CommandGenerator();

		 internal virtual TransactionToApply NextTransaction( long txId )
		 {
			  PhysicalTransactionRepresentation representation = new PhysicalTransactionRepresentation( CreateRandomCommands() );
			  TransactionHeaderInformation headerInfo = DEFAULT.create();
			  representation.SetHeader( headerInfo.AdditionalHeader, headerInfo.MasterId, headerInfo.AuthorId, headerInfo.AuthorId, txId, currentTimeMillis(), 42 );
			  return new TransactionToApply( representation );
		 }

		 private ICollection<StorageCommand> CreateRandomCommands()
		 {
			  int commandNum = ThreadLocalRandom.current().Next(1, 17);
			  IList<StorageCommand> commands = new List<StorageCommand>( commandNum );
			  for ( int i = 0; i < commandNum; i++ )
			  {
					commands.Add( _commandGenerator.nextCommand() );
			  }
			  return commands;
		 }

		 private class CommandGenerator
		 {
			  internal NodeRecordGenerator NodeRecordGenerator = new NodeRecordGenerator();

			  internal virtual Command NextCommand()
			  {
					return new Command.NodeCommand( NodeRecordGenerator.nextRecord(), NodeRecordGenerator.nextRecord() );
			  }
		 }

		 private class NodeRecordGenerator
		 {

			  internal virtual NodeRecord NextRecord()
			  {
					ThreadLocalRandom random = ThreadLocalRandom.current();
					return new NodeRecord( random.nextLong(), random.nextBoolean(), random.nextBoolean(), random.nextLong(), random.nextLong(), random.nextLong() );
			  }
		 }
	}

}