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
namespace Neo4Net.cluster.protocol.atomicbroadcast
{

	public class LenientObjectInputStream : ObjectInputStream
	{
		 private VersionMapper _versionMapper;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LenientObjectInputStream(java.io.ByteArrayInputStream fis, VersionMapper versionMapper) throws java.io.IOException
		 public LenientObjectInputStream( MemoryStream fis, VersionMapper versionMapper ) : base( fis )
		 {
			  this._versionMapper = versionMapper;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.io.ObjectStreamClass readClassDescriptor() throws java.io.IOException, ClassNotFoundException
		 protected internal override ObjectStreamClass ReadClassDescriptor()
		 {
			  ObjectStreamClass wireClassDescriptor = base.ReadClassDescriptor();
			  if ( !_versionMapper.hasMappingFor( wireClassDescriptor.Name ) )
			  {
					_versionMapper.addMappingFor( wireClassDescriptor.Name, wireClassDescriptor.SerialVersionUID );
			  }

			  Type localClass; // the class in the local JVM that this descriptor represents.
			  try
			  {
					localClass = Type.GetType( wireClassDescriptor.Name );
			  }
			  catch ( ClassNotFoundException )
			  {
					return wireClassDescriptor;
			  }
			  ObjectStreamClass localClassDescriptor = ObjectStreamClass.lookup( localClass );
			  if ( localClassDescriptor != null )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long localSUID = localClassDescriptor.getSerialVersionUID();
					long localSUID = localClassDescriptor.SerialVersionUID;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long wireSUID = wireClassDescriptor.getSerialVersionUID();
					long wireSUID = wireClassDescriptor.SerialVersionUID;
					if ( wireSUID != localSUID )
					{
						 wireClassDescriptor = localClassDescriptor;
					}
			  }
			  return wireClassDescriptor;
		 }
	}

}