using System;

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
namespace Neo4Net.management
{

	[Obsolete, Serializable]
	public sealed class BranchedStoreInfo
	{
		 private const long SERIAL_VERSION_UID = -3519343870927764106L;

		 private string _directory;

		 private long _largestTxId;
		 private long _creationTime;
		 private long _branchedStoreSize;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ConstructorProperties({"directory", "largestTxId", "creationTime"}) public BranchedStoreInfo(String directory, long largestTxId, long creationTime)
		 public BranchedStoreInfo( string directory, long largestTxId, long creationTime ) : this( directory, largestTxId, creationTime, 0 )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ConstructorProperties({"directory", "largestTxId", "creationTime", "storeSize"}) public BranchedStoreInfo(String directory, long largestTxId, long creationTime, long branchedStoreSize)
		 public BranchedStoreInfo( string directory, long largestTxId, long creationTime, long branchedStoreSize )
		 {
			  this._directory = directory;
			  this._largestTxId = largestTxId;
			  this._creationTime = creationTime;
			  this._branchedStoreSize = branchedStoreSize;
		 }

		 public string Directory
		 {
			 get
			 {
				  return _directory;
			 }
		 }

		 public long LargestTxId
		 {
			 get
			 {
				  return _largestTxId;
			 }
		 }

		 public long CreationTime
		 {
			 get
			 {
				  return _creationTime;
			 }
		 }

		 public long BranchedStoreSize
		 {
			 get
			 {
				  return _branchedStoreSize;
			 }
		 }
	}

}