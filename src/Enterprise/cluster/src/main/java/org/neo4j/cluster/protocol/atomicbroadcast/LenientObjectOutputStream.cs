using System;

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

	public class LenientObjectOutputStream : ObjectOutputStream
	{
		 private VersionMapper _versionMapper;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LenientObjectOutputStream(java.io.ByteArrayOutputStream bout, VersionMapper versionMapper) throws java.io.IOException
		 public LenientObjectOutputStream( MemoryStream bout, VersionMapper versionMapper ) : base( bout )
		 {
			  this._versionMapper = versionMapper;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeClassDescriptor(java.io.ObjectStreamClass desc) throws java.io.IOException
		 protected internal override void WriteClassDescriptor( ObjectStreamClass desc )
		 {
			  if ( _versionMapper.hasMappingFor( desc.Name ) )
			  {
					UpdateWirePayloadSuid( desc );
			  }

			  base.WriteClassDescriptor( desc );
		 }

		 private void UpdateWirePayloadSuid( ObjectStreamClass wirePayload )
		 {
			  try
			  {
					System.Reflection.FieldInfo field = GetAccessibleSuidField( wirePayload );
					field.set( wirePayload, _versionMapper.mappingFor( wirePayload.Name ) );
			  }
			  catch ( Exception e ) when ( e is NoSuchFieldException || e is IllegalAccessException )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Field getAccessibleSuidField(java.io.ObjectStreamClass localClassDescriptor) throws NoSuchFieldException
		 private System.Reflection.FieldInfo GetAccessibleSuidField( ObjectStreamClass localClassDescriptor )
		 {
			  System.Reflection.FieldInfo suidField = localClassDescriptor.GetType().getDeclaredField("suid");
			  suidField.Accessible = true;
			  return suidField;
		 }
	}

}