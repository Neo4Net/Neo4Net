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
namespace Neo4Net.cluster.protocol.atomicbroadcast
{

	/// <summary>
	/// Stream factory for serializing/deserializing messages.
	/// </summary>
	public class ObjectStreamFactory : ObjectInputStreamFactory, ObjectOutputStreamFactory
	{
		 private readonly VersionMapper _versionMapper;

		 public ObjectStreamFactory()
		 {
			  _versionMapper = new VersionMapper();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.ObjectOutputStream create(java.io.ByteArrayOutputStream bout) throws java.io.IOException
		 public override ObjectOutputStream Create( MemoryStream bout )
		 {
			  return new LenientObjectOutputStream( bout, _versionMapper );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.ObjectInputStream create(java.io.ByteArrayInputStream in) throws java.io.IOException
		 public override ObjectInputStream Create( MemoryStream @in )
		 {
			  return new LenientObjectInputStream( @in, _versionMapper );
		 }
	}

}