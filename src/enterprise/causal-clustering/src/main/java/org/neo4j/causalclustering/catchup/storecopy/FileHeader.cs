/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.storecopy
{

	public class FileHeader
	{
		 private readonly string _fileName;
		 private readonly int _requiredAlignment;

		 public FileHeader( string fileName ) : this( fileName, 1 )
		 {
			  // A required alignment of 1 basically means that any alignment will do.
		 }

		 public FileHeader( string fileName, int requiredAlignment )
		 {
			  this._fileName = fileName;
			  this._requiredAlignment = requiredAlignment;
		 }

		 public virtual string FileName()
		 {
			  return _fileName;
		 }

		 public virtual int RequiredAlignment()
		 {
			  return _requiredAlignment;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  FileHeader that = ( FileHeader ) o;
			  return _requiredAlignment == that._requiredAlignment && Objects.Equals( _fileName, that._fileName );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _fileName, _requiredAlignment );
		 }

		 public override string ToString()
		 {
			  return "FileHeader{" + "fileName='" + _fileName + '\'' + ", requiredAlignment=" + _requiredAlignment + '}';
		 }
	}

}