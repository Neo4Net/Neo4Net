using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	public abstract class Headers
	{
		 public abstract Value get<Value>( HeaderField<Value> field );

		 public static Builder HeadersBuilder()
		 {
			  return new Builder( new Dictionary<>() );
		 }

		 public class Builder
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<HeaderField<?>, Object> headers;
			  internal readonly IDictionary<HeaderField<object>, object> HeadersConflict;

			  internal Builder<T1>( IDictionary<T1> headers )
			  {
					this.HeadersConflict = headers;
			  }

			  public Builder Put<Value>( HeaderField<Value> field, Value value )
			  {
					HeadersConflict[field] = value;
					return this;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public final <Value> Value get(HeaderField<Value> field)
			  public Value Get<Value>( HeaderField<Value> field )
			  {
					return ( Value ) HeadersConflict[field];
			  }

			  public virtual Headers Headers()
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new Simple(new java.util.HashMap<>(headers));
					return new Simple( new Dictionary<HeaderField<object>, object>( HeadersConflict ) );
			  }
		 }

		 internal virtual void Write<Value>( HeaderField<Value> field, BigEndianByteArrayBuffer target )
		 {
			  field.Write( Get( field ), target );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public abstract java.util.Set<HeaderField<?>> fields();
		 public abstract ISet<HeaderField<object>> Fields();

		 private Headers()
		 {
			  // internal subclasses
		 }

		 internal static Headers IndexedHeaders<T1>( IDictionary<T1> indexes, object[] values )
		 {
			  return new Indexed( indexes, values );
		 }

		 public override sealed int GetHashCode()
		 {
			  int hash = 0;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (HeaderField<?> field : fields())
			  foreach ( HeaderField<object> field in Fields() )
			  {
					hash ^= field.GetHashCode();
			  }
			  return hash;
		 }

		 public override sealed bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  if ( obj is Headers )
			  {
					Headers that = ( Headers ) obj;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<HeaderField<?>> these = this.fields();
					IEnumerable<HeaderField<object>> these = this.Fields();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<HeaderField<?>> those = that.fields();
					IEnumerable<HeaderField<object>> those = that.Fields();
					if ( these.Equals( those ) )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (HeaderField<?> field : these)
						 foreach ( HeaderField<object> field in these )
						 {
							  object tis = this.Get( field );
							  object tat = that.Get( field );
							  if ( !tis.Equals( tat ) )
							  {
									return false;
							  }
						 }
						 return true;
					}
			  }
			  return false;
		 }

		 public override sealed string ToString()
		 {
			  StringBuilder result = ( new StringBuilder() ).Append("Headers{");
			  string pre = "";
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (HeaderField<?> field : fields())
			  foreach ( HeaderField<object> field in Fields() )
			  {
					result.Append( pre ).Append( field ).Append( ": " ).Append( Get( field ) );
					pre = ", ";
			  }
			  return result.Append( "}" ).ToString();
		 }

		 private class Indexed : Headers
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<HeaderField<?>, int> indexes;
			  internal readonly IDictionary<HeaderField<object>, int> Indexes;
			  internal readonly object[] Values;

			  internal Indexed<T1>( IDictionary<T1> indexes, object[] values )
			  {
					this.Indexes = indexes;
					this.Values = values;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public <Value> Value get(HeaderField<Value> field)
			  public override Value Get<Value>( HeaderField<Value> field )
			  {
					int? index = Indexes[field];
					return index == null ? null : ( Value ) Values[index];
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Set<HeaderField<?>> fields()
			  public override ISet<HeaderField<object>> Fields()
			  {
					return Indexes.Keys;
			  }
		 }

		 private class Simple : Headers
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<HeaderField<?>, Object> headers;
			  internal readonly IDictionary<HeaderField<object>, object> Headers;

			  internal Simple<T1>( IDictionary<T1> headers )
			  {
					this.Headers = headers;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public <Value> Value get(HeaderField<Value> field)
			  public override Value Get<Value>( HeaderField<Value> field )
			  {
					return ( Value ) Headers[field];
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Set<HeaderField<?>> fields()
			  public override ISet<HeaderField<object>> Fields()
			  {
					return Headers.Keys;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: static java.util.Map<HeaderField<?>, Object> copy(Headers headers)
		 internal static IDictionary<HeaderField<object>, object> Copy( Headers headers )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<HeaderField<?>, Object> copy = new java.util.HashMap<>();
			  IDictionary<HeaderField<object>, object> copy = new Dictionary<HeaderField<object>, object>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (HeaderField<?> field : headers.fields())
			  foreach ( HeaderField<object> field in headers.Fields() )
			  {
					copy[field] = headers.Get( field );
			  }
			  return copy;
		 }
	}

}