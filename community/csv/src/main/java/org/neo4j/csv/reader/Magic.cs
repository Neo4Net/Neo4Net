using System;
using System.Collections.Generic;
using System.IO;

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
namespace Org.Neo4j.Csv.Reader
{

	/// <summary>
	/// Logic for detecting and matching magic numbers in file headers.
	/// </summary>
	public class Magic
	{
		 public static readonly Magic None = new Magic( "NONE", null, new sbyte[0] );

		 private static readonly IList<Magic> _definitions = new List<Magic>();
		 private static int _longest;

		 /// <summary>
		 /// First 4 bytes of a ZIP file have this signature. </summary>
		 public static readonly Magic Zip = Magic.Define( "ZIP", null, 0x50, 0x4b, 0x03, 0x04 );
		 /// <summary>
		 /// First 2 bytes of a GZIP file have this signature. </summary>
		 public static readonly Magic Gzip = Magic.Define( "GZIP", null, 0x1f, 0x8b );

		 /// <summary>
		 /// A couple of BOM magics </summary>
		 public static readonly Magic BomUtf_32Be = Define( "BOM_UTF_32_BE", forName( "UTF-32" ), 0x0, 0x0, 0xFE, 0xFF );
		 public static readonly Magic BomUtf_32Le = Define( "BOM_UTF_32_LE", forName( "UTF-32" ), 0xFF, 0xFE, 0x0, 0x0 );
		 public static readonly Magic BomUtf_16Be = Define( "BOM_UTF_16_BE", StandardCharsets.UTF_16BE, 0xFE, 0xFF );
		 public static readonly Magic BomUtf_16Le = Define( "BOM_UTF_16_LE", StandardCharsets.UTF_16LE, 0xFF, 0xFE );
		 public static readonly Magic BomUtf_8 = Define( "BOM_UTF8", StandardCharsets.UTF_8, 0xEF, 0xBB, 0xBF );

		 /// <summary>
		 /// Defines a magic signature which can later be detected in <seealso cref="of(File)"/> and <seealso cref="of(sbyte[])"/>.
		 /// </summary>
		 /// <param name="description"> description of the magic, typically which file it is. </param>
		 /// <param name="impliesEncoding"> if a match for this to-be-defined magic implies that the contents in
		 /// this file has a certain encoding. Typically used for byte-order-mark. </param>
		 /// <param name="bytesAsIntsForConvenience"> bytes that makes up the magic signature. Here specified as
		 /// an {@code int[]} for convenience of specifying those. </param>
		 /// <returns> the defined <seealso cref="Magic"/> instance. </returns>
		 public static Magic Define( string description, Charset impliesEncoding, params int[] bytesAsIntsForConvenience )
		 {
			  sbyte[] bytes = new sbyte[bytesAsIntsForConvenience.Length];
			  for ( int i = 0; i < bytes.Length; i++ )
			  {
					bytes[i] = ( sbyte ) bytesAsIntsForConvenience[i];
			  }

			  Magic magic = new Magic( description, impliesEncoding, bytes );
			  _definitions.Add( magic );
			  _longest = Math.Max( _longest, bytes.Length );
			  return magic;
		 }

		 /// <summary>
		 /// Extracts and matches the magic of the header in the given {@code file}. If no magic matches
		 /// then <seealso cref="NONE"/> is returned.
		 /// </summary>
		 /// <param name="file"> <seealso cref="File"/> to extract the magic from. </param>
		 /// <returns> matching <seealso cref="Magic"/>, or <seealso cref="NONE"/> if no match. </returns>
		 /// <exception cref="IOException"> for errors reading from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static Magic of(java.io.File file) throws java.io.IOException
		 public static Magic Of( File file )
		 {
			  try
			  {
					  using ( Stream @in = new FileStream( file, FileMode.Open, FileAccess.Read ) )
					  {
						sbyte[] bytes = new sbyte[_longest];
						int read = @in.Read( bytes, 0, bytes.Length );
						if ( read > 0 )
						{
							 bytes = Arrays.copyOf( bytes, read );
							 return Of( bytes );
						}
					  }
			  }
			  catch ( EOFException )
			  { // This is OK
			  }
			  return Magic.None;
		 }

		 /// <summary>
		 /// Matches the magic bytes with all defined and returns a match or <seealso cref="NONE"/>.
		 /// </summary>
		 /// <param name="bytes"> magic bytes extracted from a file header. </param>
		 /// <returns> matching <seealso cref="Magic"/>, or <seealso cref="NONE"/> if no match. </returns>
		 public static Magic Of( sbyte[] bytes )
		 {
			  foreach ( Magic candidate in _definitions )
			  {
					if ( candidate.Matches( bytes ) )
					{
						 return candidate;
					}
			  }
			  return None;
		 }

		 public static int Longest()
		 {
			  return _longest;
		 }

		 private readonly string _description;
		 private readonly Charset _encoding;
		 private readonly sbyte[] _bytes;

		 private Magic( string description, Charset encoding, sbyte[] bytes )
		 {
			  this._description = description;
			  this._encoding = encoding;
			  this._bytes = bytes;
		 }

		 /// <summary>
		 /// Tests whether or not a set of magic bytes matches this <seealso cref="Magic"/> signature.
		 /// </summary>
		 /// <param name="candidateBytes"> magic bytes to test. </param>
		 /// <returns> {@code true} if the candidate bytes matches this signature, otherwise {@code false}. </returns>
		 public virtual bool Matches( sbyte[] candidateBytes )
		 {
			  if ( candidateBytes.Length < _bytes.Length )
			  {
					return false;
			  }
			  for ( int i = 0; i < _bytes.Length; i++ )
			  {
					if ( candidateBytes[i] != _bytes[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 /// <returns> number of bytes making up this magic signature. </returns>
		 public virtual int Length()
		 {
			  return _bytes.Length;
		 }

		 /// <returns> whether or not the presence of this <seealso cref="Magic"/> implies the contents of the file being
		 /// of a certain encoding. If {@code true} then <seealso cref="encoding()"/> may be called to get the implied encoding. </returns>
		 public virtual bool ImpliesEncoding()
		 {
			  return _encoding != null;
		 }

		 /// <returns> the encoding this magic signature implies, if <seealso cref="impliesEncoding()"/> is {@code true},
		 /// otherwise throws <seealso cref="System.InvalidOperationException"/>. </returns>
		 public virtual Charset Encoding()
		 {
			  if ( _encoding == null )
			  {
					throw new System.InvalidOperationException( this + " doesn't imply any specific encoding" );
			  }
			  return _encoding;
		 }

		 internal virtual sbyte[] Bytes()
		 {
			  // Defensive copy
			  return Arrays.copyOf( _bytes, _bytes.Length );
		 }

		 public override string ToString()
		 {
			  return "Magic[" + _description + ", " + Arrays.ToString( _bytes ) + "]";
		 }
	}

}