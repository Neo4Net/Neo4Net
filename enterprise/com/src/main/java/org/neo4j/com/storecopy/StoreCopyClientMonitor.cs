﻿/*
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
namespace Org.Neo4j.com.storecopy
{
	public interface StoreCopyClientMonitor
	{
		 void StartReceivingStoreFiles();

		 void FinishReceivingStoreFiles();

		 void StartReceivingStoreFile( string file );

		 void FinishReceivingStoreFile( string file );

		 void StartReceivingTransactions( long startTxId );

		 void FinishReceivingTransactions( long endTxId );

		 void StartRecoveringStore();

		 void FinishRecoveringStore();

		 void StartReceivingIndexSnapshots();

		 void StartReceivingIndexSnapshot( long indexId );

		 void FinishReceivingIndexSnapshot( long indexId );

		 void FinishReceivingIndexSnapshots();
	}

	 public class StoreCopyClientMonitor_Adapter : StoreCopyClientMonitor
	 {
		  public override void StartReceivingStoreFiles()
		  { // empty
		  }

		  public override void FinishReceivingStoreFiles()
		  { // empty
		  }

		  public override void StartReceivingStoreFile( string file )
		  { // empty
		  }

		  public override void FinishReceivingStoreFile( string file )
		  { // empty
		  }

		  public override void StartReceivingTransactions( long startTxId )
		  { // empty
		  }

		  public override void FinishReceivingTransactions( long endTxId )
		  { // empty
		  }

		  public override void StartRecoveringStore()
		  { // empty
		  }

		  public override void FinishRecoveringStore()
		  { // empty
		  }

		  public override void StartReceivingIndexSnapshots()
		  { // empty
		  }

		  public override void StartReceivingIndexSnapshot( long indexId )
		  { // empty
		  }

		  public override void FinishReceivingIndexSnapshot( long indexId )
		  { // empty
		  }

		  public override void FinishReceivingIndexSnapshots()
		  { // empty
		  }
	 }

}