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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	/// <summary>
	/// A fake file tracks a file, but also several counters and helpers that can be used in tests to invoke desired behaviour
	/// </summary>
	internal class FakeFile
	{
		 private File _file;
		 private string _filename;
		 private string _content;
		 private int _remainingNoResponse;
		 private int _remainingFailed;
		 private Path _relativePath;

		 internal FakeFile( string name, string content )
		 {
			  Filename = name;
			  this._content = content;
		 }

		 public virtual string Filename
		 {
			 set
			 {
				  this._filename = value;
				  this._file = RelativePath.resolve( value ).toFile();
			 }
			 get
			 {
				  return _filename;
			 }
		 }

		 public virtual File File
		 {
			 set
			 {
				  this._filename = value.Name;
				  this._file = value;
			 }
			 get
			 {
				  return _file;
			 }
		 }

		 private Path RelativePath
		 {
			 get
			 {
				  return Optional.ofNullable( _relativePath ).orElse( ( new File( "." ) ).toPath() );
			 }
		 }



		 public virtual string Content
		 {
			 get
			 {
				  return _content;
			 }
			 set
			 {
				  this._content = value;
			 }
		 }


		 /// <summary>
		 /// Clear response that the file has failed to copy (safe connection close, communication, ...)
		 /// 
		 /// @return
		 /// </summary>
		 internal virtual int RemainingFailed
		 {
			 get
			 {
				  return _remainingFailed;
			 }
			 set
			 {
				  this._remainingFailed = value;
			 }
		 }

	}

}