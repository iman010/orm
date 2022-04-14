//using System;



//namespace SWE3.Demo
//{
//    /// <summary>This class implements a lazy loading wrapper for framework objects.</summary>
//    /// <typeparam name="T">Type.</typeparam>
//    public class Lazy<T>
//    {
//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        // protected members                                                                                                //
//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        /// <summary>Primary keys.</summary>
//        protected object[] _Pks;

//        /// <summary>Value.</summary>
//        protected T _Value;

//        /// <summary>Initialized flag.</summary>
//        protected bool _Initialized = false;



//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        // constructors                                                                                                     //
//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        /// <summary>Creates a new instance of this class.</summary>
//        /// <param name="pks">Primary keys.</param>
//        public Lazy(params object[] pks)
//        {
//            _Pks = pks;
//        }


//        /// <summary>Creates a new instance of this class.</summary>
//        /// <param name="obj">Object.</param>
//        public Lazy(T obj)
//        {
//            _Initialized = true;
//            _Value = obj;
//        }



//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        // public properties                                                                                                //
//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
//        /// <summary>Gets the object value.</summary>
//        public T Value
//        {
//            get
//            {
//                if(!_Initialized) { _Value = World.GetObject<T>(_Pks); }
//                return _Value;
//            }
//            set 
//            {
//                _Initialized = true;
//                _Value = value; 
//            }
//        }



//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        // operators                                                                                                        //
//        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
//        /// <summary>Implements an implicit operator for the Lazy class.</summary>
//        /// <param name="lazy">Lazy object.</param>
//        public static implicit operator T(Lazy<T> lazy)
//        {
//            return lazy._Value;
//        }


//        /// <summary>Implements an implicit operator for the Lazy class.</summary>
//        /// <param name="lazy">Lazy object.</param>
//        public static implicit operator Lazy<T>(T obj)
//        {
//            return new Lazy<T>(obj);
//        }
//    }
//}
