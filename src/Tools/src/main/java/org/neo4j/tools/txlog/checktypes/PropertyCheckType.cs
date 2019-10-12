using System.Collections.Generic;

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

	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;

	internal class PropertyCheckType : CheckType<Command.PropertyCommand, PropertyRecord>
	{
		 internal PropertyCheckType() : base(typeof(Command.PropertyCommand))
		 {
		 }

		 public override PropertyRecord Before( Command.PropertyCommand command )
		 {
			  return command.Before;
		 }

		 public override PropertyRecord After( Command.PropertyCommand command )
		 {
			  return command.After;
		 }

		 protected internal override bool InUseRecordsEqual( PropertyRecord record1, PropertyRecord record2 )
		 {
			  return record1.NodeSet == record2.NodeSet && record1.RelSet == record2.RelSet && record1.NodeId == record2.NodeId && record1.RelId == record2.RelId && record1.NextProp == record2.NextProp && record1.PrevProp == record2.PrevProp && BlocksEqual( record1, record2 );
		 }

		 public override string Name()
		 {
			  return "property";
		 }

		 private static bool BlocksEqual( PropertyRecord r1, PropertyRecord r2 )
		 {
			  if ( r1.Size() != r2.Size() )
			  {
					return false;
			  }
			  IList<PropertyBlock> r1Blocks = Blocks( r1 );
			  IList<PropertyBlock> r2Blocks = Blocks( r2 );
			  if ( r1Blocks.Count != r2Blocks.Count )
			  {
					return false;
			  }
			  for ( int i = 0; i < r1Blocks.Count; i++ )
			  {
					PropertyBlock r1Block = r1Blocks[i];
					PropertyBlock r2Block = r2Blocks[i];
					if ( !Arrays.Equals( r1Block.ValueBlocks, r2Block.ValueBlocks ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private static IList<PropertyBlock> Blocks( PropertyRecord record )
		 {
			  IList<PropertyBlock> result = new List<PropertyBlock>();
			  foreach ( PropertyBlock block in record )
			  {
					result.Add( block );
			  }
			  return result;
		 }
	}

}