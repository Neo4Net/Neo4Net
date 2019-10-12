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
namespace Org.Neo4j.com
{

	public class FailingByteChannel : KnownDataByteChannel
	{
		 private readonly string _failWithMessage;
		 private readonly int _sizeToFailAt;

		 public FailingByteChannel( int sizeToFailAt, string failWithMessage ) : base( sizeToFailAt * 2 )
		 {
			  this._sizeToFailAt = sizeToFailAt;
			  this._failWithMessage = failWithMessage;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
		 public override int Read( ByteBuffer dst )
		 {
			  if ( Position > _sizeToFailAt )
			  {
					throw new MadeUpException( _failWithMessage );
			  }
			  return base.Read( dst );
		 }
	}

}